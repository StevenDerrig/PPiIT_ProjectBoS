// Services/RoomService.cs
using BookingApp.Models;

namespace BookingApp.Services;

public class RoomService
{
    private readonly APIService _apiService;

    public RoomService(APIService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<Room>> GetAllRoomsAsync()
    {
        return await _apiService.GetAsync<List<Room>>("rooms");
    }

    public Task<List<Room>> GetAvailableRoomsAsync()
    {
        return _apiService.GetAsync<List<Room>>("rooms/available");
    }

    public async Task<Room> GetRoomByIdAsync(long id)
    {
        return await _apiService.GetAsync<Room>($"rooms/{id}");
    }

    public class BookingService
    {
        private readonly APIService _apiService;

        public BookingService(APIService apiService)
        {
            _apiService = apiService;
        }

        public Task<Booking> CreateBookingAsync(BookingCreateRequest request)
        {
            return _apiService.PostAsync<Booking>("bookings", request);
        }

        public Task<List<Booking>> GetAllBookingsAsync()
        {
            return _apiService.GetAsync<List<Booking>>("bookings");
        }

        public Task<Booking> GetBookingByIdAsync(long id)
        {
            return _apiService.GetAsync<Booking>($"bookings/{id}");
        }
    }
}