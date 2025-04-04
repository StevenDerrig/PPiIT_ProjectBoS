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

        // Load available rooms
        LoadAvailableRooms();

        // Add event handlers
        CheckInDatePicker.DateSelected += OnDateSelected;
        CheckOutDatePicker.DateSelected += OnDateSelected;
        RoomPicker.SelectedIndexChanged += OnRoomSelected;
    }

    private async void LoadAvailableRooms()
    {
        try
        {
            _availableRooms = await _roomService.GetAvailableRoomsAsync();

            // Configure the room picker
            RoomPicker.ItemsSource = _availableRooms.Select(r =>
                $"Room {r.RoomNumber} - {r.RoomType} (€{r.PricePerNight}/night)").ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load available rooms: {ex.Message}", "OK");
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

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // Ensure check-out is after check-in
        if (CheckOutDatePicker.Date <= CheckInDatePicker.Date)
        {
            CheckOutDatePicker.Date = CheckInDatePicker.Date.AddDays(1);
        }

        UpdatePricingSummary();
    }

    private void OnRoomSelected(object sender, EventArgs e)
    {
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
            var selectedRoom = _availableRooms[RoomPicker.SelectedIndex];
            var bookingRequest = new BookingCreateRequest
            {
                GuestId = (int)guest.GuestId, // Cast long to int
                RoomId = (int)selectedRoom.RoomId,
                CheckInDate = CheckInDatePicker.Date,
                CheckOutDate = CheckOutDatePicker.Date,
                BreakfastIncluded = BreakfastCheckBox.IsChecked,
                Notes = NotesEditor.Text
            };

            //Logging the booking request
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