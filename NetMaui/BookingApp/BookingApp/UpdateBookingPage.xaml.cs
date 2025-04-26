using BookingApp.Models;
using BookingApp.Services;
using System.Collections.ObjectModel;
using static BookingApp.Services.RoomService;

namespace BookingApp;

public partial class UpdateBookingPage : ContentPage
{
    private readonly APIService _apiService;
    private readonly BookingService _bookingService;
    private DateTime selectedDate;
    private List<Booking> dayBookings;
    private ObservableCollection<Booking> searchResults;
    private Booking selectedBooking;
    private bool HasSearchResults => searchResults != null && searchResults.Count > 0;

    public UpdateBookingPage()
    {
        InitializeComponent();

        _apiService = new APIService();
        _bookingService = new BookingService(_apiService);

        // Initialize collections
        searchResults = new ObservableCollection<Booking>();
        BookingsCollectionView.ItemsSource = searchResults;

        // Initialize date pickers with defaults
        FromDatePicker.Date = DateTime.Today;
        ToDatePicker.Date = DateTime.Today.AddDays(1);

        // Set initial visibility
        EditBookingSection.IsVisible = false;
    }

    // Constructor when opened from the calendar view
    public UpdateBookingPage(DateTime selectedDate, List<Booking> dayBookings)
    {
        InitializeComponent();

        _apiService = new APIService();
        _bookingService = new BookingService(_apiService);

        this.selectedDate = selectedDate;
        this.dayBookings = dayBookings;

        // Initialize with provided bookings
        searchResults = new ObservableCollection<Booking>(dayBookings);
        BookingsCollectionView.ItemsSource = searchResults;

        // Initialize date pickers with defaults
        FromDatePicker.Date = selectedDate;
        ToDatePicker.Date = selectedDate.AddDays(1);

        // Set initial visibility
        EditBookingSection.IsVisible = false;
    }

    private async void OnSearchButtonClicked(object sender, EventArgs e)
    {
        if (SearchTypePicker.SelectedIndex == -1 || string.IsNullOrWhiteSpace(SearchEntry.Text))
        {
            await DisplayAlert("Invalid Search", "Please select search criteria and enter a search value", "OK");
            return;
        }

        try
        {
            // Clear previous results
            searchResults.Clear();

            // Get search type and value
            string searchType = SearchTypePicker.SelectedItem.ToString();
            string searchValue = SearchEntry.Text.Trim();

            List<Booking> results;

            switch (searchType)
            {
                case "Booking ID":
                    if (int.TryParse(searchValue, out int bookingId))
                    {
                        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                        if (booking != null)
                        {
                            results = new List<Booking> { booking };
                        }
                        else
                        {
                            results = new List<Booking>();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Invalid ID", "Please enter a valid booking ID number", "OK");
                        return;
                    }
                    break;

                case "Guest Name":
                    // Search by guest name from API
                    results = await _apiService.GetAsync<List<Booking>>($"bookings/search?guestName={searchValue}");
                    break;

                case "Contact Number":
                    // Search by contact number from API
                    results = await _apiService.GetAsync<List<Booking>>($"bookings/search?contactNumber={searchValue}");
                    break;

                case "Room Number":
                    // Search by room number from API
                    results = await _apiService.GetAsync<List<Booking>>($"bookings/search?roomNumber={searchValue}");
                    break;

                default:
                    results = new List<Booking>();
                    break;
            }

            // Update results
            foreach (var booking in results)
            {
                searchResults.Add(booking);
            }

            // Update visibility based on results
            BookingsCollectionView.IsVisible = true;

            // Notify user if no results found
            if (searchResults.Count == 0)
            {
                await DisplayAlert("No Results", "No bookings match your search criteria", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while searching: {ex.Message}", "OK");
            Console.WriteLine($"Search error: {ex}");
        }
    }

    private async void OnBrowseButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // Clear previous results
            searchResults.Clear();

            // Get date range
            DateTime fromDate = FromDatePicker.Date;
            DateTime toDate = ToDatePicker.Date;

            if (toDate < fromDate)
            {
                await DisplayAlert("Invalid Dates", "To date must be after From date", "OK");
                return;
            }

            // Get bookings in date range
            var results = await _apiService.GetAsync<List<Booking>>(
                $"bookings/date-range?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

            // Update results
            foreach (var booking in results)
            {
                searchResults.Add(booking);
            }

            // Update visibility based on results
            BookingsCollectionView.IsVisible = true;

            // Notify user if no results found
            if (searchResults.Count == 0)
            {
                await DisplayAlert("No Results", "No bookings found in the selected date range", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while browsing: {ex.Message}", "OK");
            Console.WriteLine($"Browse error: {ex}");
        }
    }

    private void OnBookingSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Booking booking)
        {
            selectedBooking = booking;

            // Populate edit section
            BookingIdLabel.Text = booking.BookingId.ToString();
            GuestNameLabel.Text = $"{booking.Guest.FirstName} {booking.Guest.LastName}";
            RoomNumberLabel.Text = booking.Room.RoomNumber;
            BookingStatusLabel.Text = booking.BookingStatus;

            // Set editing fields
            EditCheckInDatePicker.Date = booking.CheckInDate;
            EditCheckOutDatePicker.Date = booking.CheckOutDate;
            EditBreakfastCheckBox.IsChecked = booking.BreakfastIncluded;
            EditNotesEditor.Text = booking.Notes;

            // Show edit section
            EditBookingSection.IsVisible = true;
        }
    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        if (selectedBooking == null)
        {
            return;
        }

        try
        {
            // Validate dates
            if (EditCheckOutDatePicker.Date <= EditCheckInDatePicker.Date)
            {
                await DisplayAlert("Invalid Dates", "Check-out date must be after check-in date", "OK");
                return;
            }

            // Create update request object
            var updateRequest = new
            {
                bookingId = selectedBooking.BookingId,
                checkInDate = EditCheckInDatePicker.Date,
                checkOutDate = EditCheckOutDatePicker.Date,
                breakfastIncluded = EditBreakfastCheckBox.IsChecked,
                notes = EditNotesEditor.Text,
                lastModified = DateTime.Now // This will get added to the database
            };

            // Call API to update booking
            await _apiService.PutAsync<Booking>($"bookings/{selectedBooking.BookingId}", updateRequest);

            await DisplayAlert("Success", "Booking updated successfully", "OK");

            // Refresh the booking list
            if (searchResults.Contains(selectedBooking))
            {
                // Update the item in the collection
                var index = searchResults.IndexOf(selectedBooking);
                selectedBooking.CheckInDate = EditCheckInDatePicker.Date;
                selectedBooking.CheckOutDate = EditCheckOutDatePicker.Date;
                selectedBooking.BreakfastIncluded = EditBreakfastCheckBox.IsChecked;
                selectedBooking.Notes = EditNotesEditor.Text;

                // Replace the item to refresh the view
                searchResults[index] = selectedBooking;
            }

            // Hide edit section
            EditBookingSection.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update booking: {ex.Message}", "OK");
            Console.WriteLine($"Update error: {ex}");
        }
    }

    private async void OnDeleteBookingClicked(object sender, EventArgs e)
    {
        if (selectedBooking == null)
        {
            return;
        }

        // Confirm deletion
        bool confirm = await DisplayAlert("Confirm Deletion",
            $"Are you sure you want to delete the booking for {selectedBooking.Guest.FirstName} {selectedBooking.Guest.LastName}?",
            "Yes", "No");

        if (!confirm)
        {
            return;
        }

        try
        {
            // Call API to delete booking
            await _apiService.DeleteAsync($"bookings/{selectedBooking.BookingId}");

            await DisplayAlert("Success", "Booking deleted successfully", "OK");

            // Remove from the collection
            if (searchResults.Contains(selectedBooking))
            {
                searchResults.Remove(selectedBooking);
            }

            // Hide edit section
            EditBookingSection.IsVisible = false;
            selectedBooking = null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete booking: {ex.Message}", "OK");
            Console.WriteLine($"Delete error: {ex}");
        }
    }
}