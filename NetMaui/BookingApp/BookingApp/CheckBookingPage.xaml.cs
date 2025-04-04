using BookingApp.Models;
using BookingApp.Services;

namespace BookingApp;

public partial class CheckBookingPage : ContentPage
{
	private readonly RoomService _roomService;
	private List<Room> _rooms;
    public CheckBookingPage()
	{
		InitializeComponent();
        var apiService = new APIService();
        _roomService = new RoomService(apiService);

        LoadRooms();
    }

    private async void LoadRooms()
    {
        try
        {
            _rooms = await _roomService.GetAllRoomsAsync();
            RoomsListView.ItemsSource = _rooms;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load rooms: {ex.Message}", "OK");
        }
    }
}