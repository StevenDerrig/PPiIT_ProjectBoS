using BookingApp.Models;
using BookingApp.Services;

using static BookingApp.Services.RoomService;

namespace BookingApp;

public partial class MakeBookingPage : ContentPage
{
    private readonly RoomService _roomService;
    private readonly BookingService _bookingService;
    private readonly APIService _apiService;
    private List<Room> _availableRooms;

    public DateTime Today { get; } = DateTime.Today;
    public DateTime Tomorrow { get; } = DateTime.Today.AddDays(1);
    public MakeBookingPage()
    {
        InitializeComponent();

        _apiService = new APIService();
        _roomService = new RoomService(_apiService);
        _bookingService = new BookingService(_apiService);

        // Set initial dates
        CheckInDatePicker.Date = Today;
        CheckOutDatePicker.Date = Tomorrow;

        // Set binding context for date pickers
        BindingContext = this;

        // Load all rooms for the dropdown
        LoadAllRooms();

        // Add event handlers
        CheckInDatePicker.DateSelected += OnDateSelected;
        CheckOutDatePicker.DateSelected += OnDateSelected;
        RoomPicker.SelectedIndexChanged += OnRoomSelected;
    }

    private async void LoadAllRooms()
    {
        try
        {
            _availableRooms = await _roomService.GetAllRoomsAsync();

            // Configure the room picker
            RoomPicker.ItemsSource = _availableRooms.Select(r =>
                $"Room {r.RoomNumber} - {r.RoomType} (€{r.PricePerNight}/night)").ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load available rooms: {ex.Message}", "OK");
        }
    }

    // Check if the selected room is available for the selected dates
    private async Task<bool> CheckRoomAvailability(int roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        try
        {
            string endpoint = $"rooms/available-for-dates?roomId={roomId}&checkInDate={checkInDate:yyyy-MM-dd}&checkOutDate={checkOutDate:yyyy-MM-dd}";
            var availableRooms = await _apiService.GetAsync<List<Room>>(endpoint);

            bool isAvailable = availableRooms.Any(r => r.RoomId == roomId);
            Console.WriteLine($"Room {roomId} availability for dates {checkInDate:yyyy-MM-dd} to {checkOutDate:yyyy-MM-dd}: {isAvailable}");
            return isAvailable;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking room availability: {ex.Message}");
            return false;
        }
    }

    private void UpdatePricingSummary()
    {
        if (RoomPicker.SelectedIndex == -1) return;

        var selectedRoom = _availableRooms[RoomPicker.SelectedIndex];
        var checkInDate = CheckInDatePicker.Date;
        var checkOutDate = CheckOutDatePicker.Date;

        // Calculate the number of nights
        var nights = (int)(checkOutDate - checkInDate).TotalDays;
        nights = Math.Max(1, nights); // Ensure at least 1 night

        // Update the UI
        NightsLabel.Text = $"Number of nights: {nights}";
        PricePerNightLabel.Text = $"Price per night: €{selectedRoom.PricePerNight}";

        // Calculate total
        var total = selectedRoom.PricePerNight * nights;
        TotalPriceLabel.Text = $"Total price: €{total}";
    }

    private async void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // Ensure check-out is after check-in
        if (CheckOutDatePicker.Date <= CheckInDatePicker.Date)
        {
            CheckOutDatePicker.Date = CheckInDatePicker.Date.AddDays(1);
        }
        // If a room is selected, check if it's available for the new dates
        if (RoomPicker.SelectedIndex != -1)
        {
            var selectedRoom = _availableRooms[RoomPicker.SelectedIndex];
            bool isAvailable = await CheckRoomAvailability(selectedRoom.RoomId,
                                                       CheckInDatePicker.Date,
                                                       CheckOutDatePicker.Date);

            if (!isAvailable)
            {
                await DisplayAlert("Date Conflict", $"Room {selectedRoom.RoomNumber} is not available for these dates.", "OK");
            }
        }

        UpdatePricingSummary();
    }

    private async void OnRoomSelected(object sender, EventArgs e)
    {
        // Check if the selected room is available for the selected dates
        if (RoomPicker.SelectedIndex == -1) return;
        var selectedRoom = _availableRooms[RoomPicker.SelectedIndex];

        // Get booking info regarding the selected room on that date
        if (selectedRoom.Status.ToLower() == "occupied")
        {
            try
            {
                // Check dates selected
                var checkInDate = CheckInDatePicker.Date;
                var checkOutDate = CheckOutDatePicker.Date;

                bool isAvailable = await CheckRoomAvailability(selectedRoom.RoomId, checkInDate, checkOutDate);
                if (!isAvailable)
                {
                    // Get bookings for this room
                    var bookings = await _apiService.GetAsync<List<Booking>>($"bookings/room/{selectedRoom.RoomId}");

                    // Find bookings that overlap with selected dates
                    var overlappingBooking = bookings.Where(b => (b.CheckInDate <= checkOutDate && b.CheckOutDate >= checkInDate) && b.BookingStatus != "cancelled").ToList();

                    // Check and get the overlapping booking
                    if (overlappingBooking.Any())
                    {
                        var booking = overlappingBooking.First();
                        string message = $"Room {selectedRoom.RoomNumber} is already booked:\n" +
                                        $"Guest: {booking.Guest.FirstName} {booking.Guest.LastName}\n" +
                                        $"Check-in: {booking.CheckInDate:d}\n" +
                                        $"Check-out: {booking.CheckOutDate:d}\n\n" +
                                        "Would you like to select a different room or change your dates?";

                        bool changeRoom = await DisplayAlert("Room Not Available", message,
                                                          "Select Different Room", "Change Dates");

                        if (changeRoom)
                        {
                            // Reset the room selection
                            RoomPicker.SelectedIndex = -1;
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking room availability: {ex.Message}");
                await DisplayAlert("Error", $"Failed to check room availability: {ex.Message}", "OK");
            }
        }

        UpdatePricingSummary();
    }

    private async void OnCreateBookingClicked(object sender, EventArgs e)
    {
        if (RoomPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Error", "Please select a room", "OK");
            return;
        }
        // Check if all the required fields are filled in
        if (string.IsNullOrEmpty(FirstNameEntry.Text) || string.IsNullOrEmpty(LastNameEntry.Text) || string.IsNullOrEmpty(ContactNumberEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all required guest information", "OK");
            return;
        }

        var selectedRoom = _availableRooms[RoomPicker.SelectedIndex];
        var checkInDate = CheckInDatePicker.Date;
        var checkOutDate = CheckOutDatePicker.Date;

        // Another availability check before creating the booking
        bool isAvailable = await CheckRoomAvailability(selectedRoom.RoomId, checkInDate, checkOutDate);

        if (!isAvailable)
        {
            await DisplayAlert("Room Not Available", $"Room {selectedRoom.RoomNumber} is no longer available for the selected dates. Choose different room or dates.", "OK");
            return;
        }

        try
        {
            CreateBookingButton.IsEnabled = false;

            // First create or find guest
            var guestRequest = new
            {
                firstName = FirstNameEntry.Text,
                lastName = LastNameEntry.Text,
                contactNumber = ContactNumberEntry.Text,
            };

            Console.WriteLine($"Sending guest data: {System.Text.Json.JsonSerializer.Serialize(guestRequest)}");

            // Use the find-or-create endpoint
            var guest = await _apiService.PostAsync<Guest>("guests/find-or-create", guestRequest);
            Console.WriteLine($"Received guest: {System.Text.Json.JsonSerializer.Serialize(guest)}");

            // Create booking request
            var bookingRequest = new BookingCreateRequest
            {
                GuestId = (int)guest.GuestId, // Cast long to int
                RoomId = (int)selectedRoom.RoomId,
                CheckInDate = CheckInDatePicker.Date,
                CheckOutDate = CheckOutDatePicker.Date,
                BreakfastIncluded = BreakfastCheckBox.IsChecked,
                Notes = NotesEditor.Text?.Replace("\r\n", " | ").Replace("\n", " | ")// Better able to hanlde multiple lines for the db
            };

            // Logging the booking request
            var bookingJson = System.Text.Json.JsonSerializer.Serialize(bookingRequest);
            Console.WriteLine($"Sending booking data: {bookingJson}");

            // Submit booking
            var booking = await _bookingService.CreateBookingAsync(bookingRequest);
            await DisplayAlert("Success", $"Booking created successfully! Booking ID: {booking.BookingId}", "OK");
            Console.WriteLine($"Sending booking: {System.Text.Json.JsonSerializer.Serialize(booking)}");

            // Navigate back or clear form
            await Navigation.PopAsync();
        }

        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create booking: {ex.Message}", "OK");
            Console.WriteLine(ex);
        }
        finally
        {
            CreateBookingButton.IsEnabled = true;
        }
    }
}