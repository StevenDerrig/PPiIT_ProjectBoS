using System.Text.Json.Serialization;

namespace BookingApp.Models;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; }
    public string RoomType { get; set; }
    public int Capacity { get; set; }
    public int PricePerNight { get; set; }
    public string Status { get; set; } = "available";
}

public class Guest
{
    [JsonPropertyName("guestId")]
    public int GuestId { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("contactNumber")]
    public string ContactNumber { get; set; }
    public DateTime? CreatedAt { get; set; }

}

public class Booking
{
    public int BookingId { get; set; }
    public Guest Guest { get; set; }
    public Room Room { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public bool BreakfastIncluded { get; set; }
    public decimal TotalPrice { get; set; }
    public string BookingStatus { get; set; } = "confirmed";
    public string Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// Simple booking creation request model
public class BookingCreateRequest
{
    [JsonPropertyName("guestId")]
    public int GuestId { get; set; }

    [JsonPropertyName("roomId")]
    public int RoomId { get; set; }
    
    [JsonPropertyName("checkInDate")]
    public DateTime CheckInDate { get; set; }
    
    [JsonPropertyName("checkOutDate")]
    public DateTime CheckOutDate { get; set; }

    [JsonPropertyName("breakfastIncluded")]
    public bool BreakfastIncluded { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; }
}
