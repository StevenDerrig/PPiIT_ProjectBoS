package com.steebo.booking.repository;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import com.steebo.booking.model.Booking;
import com.steebo.booking.model.Guest;
import com.steebo.booking.model.Room;

import java.util.Date;
import java.util.List;

@Repository
public interface BookingRepo extends JpaRepository<Booking, Integer> {
    
    // Find bookings by guest
    List<Booking> findByGuest(Guest guest);
    
    // Find bookings by room
    List<Booking> findByRoom(Room room);
    
    // Find bookings by check-in date
    List<Booking> findByCheckInDate(Date checkInDate);
    
    // Find bookings by booking status
    List<Booking> findByBookingStatus(Booking.BookingStatus status);
    
    // Find bookings by date range (overlapping)
    @Query("SELECT b FROM Booking b WHERE " +
           "(b.checkInDate <= :checkOutDate AND b.checkOutDate >= :checkInDate)")
    List<Booking> findBookingsInDateRange(
            @Param("checkInDate") Date checkInDate,
            @Param("checkOutDate") Date checkOutDate);
    
    // Find bookings by room and date range (to check room availability)
    @Query("SELECT b FROM Booking b WHERE b.room.roomId = :roomId AND " +
           "(b.checkInDate <= :checkOutDate AND b.checkOutDate >= :checkInDate) AND " +
           "b.bookingStatus != 'cancelled'")
    List<Booking> findBookingsByRoomAndDateRange(
            @Param("roomId") Integer integer,
            @Param("checkInDate") Date checkInDate,
            @Param("checkOutDate") Date checkOutDate);
    
    // Find all active bookings (not checked out or cancelled)
    @Query("SELECT b FROM Booking b WHERE b.bookingStatus IN ('confirmed', 'checked_in')")
    List<Booking> findAllActiveBookings();
    
    // Find upcoming check-ins for today
    @Query("SELECT b FROM Booking b WHERE b.checkInDate = CURRENT_DATE AND b.bookingStatus = 'confirmed'")
    List<Booking> findTodayCheckIns();
    
    // Find upcoming check-outs for today
    @Query("SELECT b FROM Booking b WHERE b.checkOutDate = CURRENT_DATE AND b.bookingStatus = 'checked_in'")
    List<Booking> findTodayCheckOuts();
}
