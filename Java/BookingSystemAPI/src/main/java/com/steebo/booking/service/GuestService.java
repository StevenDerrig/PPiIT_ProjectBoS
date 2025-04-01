package com.steebo.booking.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.steebo.booking.model.Guest;
import com.steebo.booking.repository.GuestRepo;

import java.util.List;
import java.util.Optional;

@Service
public class GuestService {

    private final GuestRepo guestRepository;

    @Autowired
    public GuestService(GuestRepo guestRepository) {
        this.guestRepository = guestRepository;
    }

    public List<Guest> getAllGuests() {
        return guestRepository.findAll();
    }

    public Optional<Guest> getGuestById(Integer id) {
        return guestRepository.findById(id);
    }

    public List<Guest> getGuestsByLastName(String lastName) {
        return guestRepository.findByLastName(lastName);
    }

    public Optional<Guest> getGuestByContactNumber(String contactNumber) {
        return guestRepository.findByContactNumber(contactNumber);
    }

    @Transactional
    public Guest saveGuest(Guest guest) {
        return guestRepository.save(guest);
    }

    @Transactional
    public void deleteGuest(Integer id) {
        guestRepository.deleteById(id);
    }

    // Find or create guest - useful for booking process
    @Transactional
    public Guest findOrCreateGuest(String firstName, String lastName, String contactNumber) {
        // Then try by contact number
        if (contactNumber != null && !contactNumber.isEmpty()) {
            Optional<Guest> existingGuest = guestRepository.findByContactNumber(contactNumber);
            if (existingGuest.isPresent()) {
                return existingGuest.get();
            }
        }
        
        // If no existing guest found, create a new one
        Guest newGuest = new Guest(firstName, lastName, contactNumber);
        return guestRepository.save(newGuest);
    }
}
