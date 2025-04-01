package com.steebo.booking.repository;


import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.steebo.booking.model.Guest;

import java.util.List;
import java.util.Optional;

@Repository
public interface GuestRepo extends JpaRepository<Guest, Integer> {
    
    // Find by last name
    List<Guest> findByLastName(String lastName);
    
    // Find by first name and last name
    List<Guest> findByFirstNameAndLastName(String firstName, String lastName);
    
    // Find by contact number
    Optional<Guest> findByContactNumber(String contactNumber);
}
