using BookingApp.Models;

namespace BookingApp;

public partial class UpdateBookingPage : ContentPage
{
    private DateTime selectedDate;
    private List<Booking> dayBookings;

    public UpdateBookingPage()
	{
		InitializeComponent();
	}

    public UpdateBookingPage(DateTime selectedDate, List<Booking> dayBookings)
    {
        this.selectedDate = selectedDate;
        this.dayBookings = dayBookings;
    }
}