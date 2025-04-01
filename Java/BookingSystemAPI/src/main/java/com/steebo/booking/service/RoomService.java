package com.steebo.booking.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.steebo.booking.model.Room;
import com.steebo.booking.repository.BookingRepo;
import com.steebo.booking.repository.RoomRepo;

import java.util.Date;
import java.util.List;
import java.util.Optional;

@Service
public class RoomService {

    private final RoomRepo roomRepository;
    private final BookingRepo bookingRepository;

    @Autowired
    public RoomService(RoomRepo roomRepository, BookingRepo bookingRepository) {
        this.roomRepository = roomRepository;
        this.bookingRepository = bookingRepository;
    }

    public List<Room> getAllRooms() {
        return roomRepository.findAll();
    }

    public Optional<Room> getRoomById(Integer id) {
        return roomRepository.findById(id);
    }

    public Optional<Room> getRoomByNumber(String roomNumber) {
        return roomRepository.findByRoomNumber(roomNumber);
    }

    public List<Room> getAvailableRooms() {
        return roomRepository.findByStatus(Room.Status.available);
    }

    public List<Room> getRoomsByType(String roomType) {
        return roomRepository.findByRoomType(roomType);
    }

    @Transactional
    public Room saveRoom(Room room) {
        return roomRepository.save(room);
    }

    @Transactional
    public void deleteRoom(Integer id) {
        roomRepository.deleteById(id);
    }

    // Check if a room is available for specific dates
    public boolean isRoomAvailable(Integer integer, Date checkInDate, Date checkOutDate) {
        // A room is available if there are no bookings that overlap with the requested dates
        return bookingRepository.findBookingsByRoomAndDateRange(integer, checkInDate, checkOutDate).isEmpty();
    }

    // Find available rooms for specific dates
    public List<Room> getAvailableRoomsForDates(Date checkInDate, Date checkOutDate) {
        List<Room> allRooms = roomRepository.findAll();
        
        // Filter out rooms that have bookings during the requested dates
        return allRooms.stream()
                .filter(room -> isRoomAvailable(room.getRoomId(), checkInDate, checkOutDate))
                .toList();
    }
}
