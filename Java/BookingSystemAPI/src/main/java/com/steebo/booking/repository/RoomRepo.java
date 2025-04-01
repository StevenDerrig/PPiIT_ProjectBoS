package com.steebo.booking.repository;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.steebo.booking.model.Room;

import java.util.List;
import java.util.Optional;

@Repository
public interface RoomRepo extends JpaRepository<Room, Integer> {
    
    // Find by room number
    Optional<Room> findByRoomNumber(String roomNumber);
    
    // Find by room type
    List<Room> findByRoomType(String roomType);
    
    // Find by status
    List<Room> findByStatus(Room.Status status);
    
    // Find by capacity
    List<Room> findByCapacityGreaterThanEqual(Integer capacity);
    
    // Find by price range
    List<Room> findByPricePerNightBetween(Integer minPrice, Integer maxPrice);
    
    // Find available rooms by type
    List<Room> findByStatusAndRoomType(Room.Status status, String roomType);
}
