package com.steebo.booking.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import com.steebo.booking.model.Guest;
import com.steebo.booking.service.GuestService;

import java.util.List;

@RestController
@RequestMapping("/api/guests")
@CrossOrigin(origins = "*") // For development - restrict in production
public class GuestController {

    private final GuestService guestService;

    @Autowired
    public GuestController(GuestService guestService) {
        this.guestService = guestService;
    }

    @GetMapping
    public ResponseEntity<List<Guest>> getAllGuests() {
        List<Guest> guests = guestService.getAllGuests();
        return new ResponseEntity<>(guests, HttpStatus.OK);
    }

    @GetMapping("/{id}")
    public ResponseEntity<Guest> getGuestById(@PathVariable Integer id) {
        return guestService.getGuestById(id)
                .map(guest -> new ResponseEntity<>(guest, HttpStatus.OK))
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/search/lastname")
    public ResponseEntity<List<Guest>> getGuestsByLastName(@RequestParam String lastName) {
        List<Guest> guests = guestService.getGuestsByLastName(lastName);
        return new ResponseEntity<>(guests, HttpStatus.OK);
    }

    @GetMapping("/search/phone")
    public ResponseEntity<Guest> getGuestByContactNumber(@RequestParam String contactNumber) {
        return guestService.getGuestByContactNumber(contactNumber)
                .map(guest -> new ResponseEntity<>(guest, HttpStatus.OK))
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @PostMapping
    public ResponseEntity<Guest> createGuest(@RequestBody Guest guest) {
        Guest savedGuest = guestService.saveGuest(guest);
        return new ResponseEntity<>(savedGuest, HttpStatus.CREATED);
    }

    @PutMapping("/{id}")
    public ResponseEntity<Guest> updateGuest(@PathVariable Integer id, @RequestBody Guest guest) {
        return guestService.getGuestById(id)
                .map(existingGuest -> {
                    guest.setGuestId(id);
                    Guest updatedGuest = guestService.saveGuest(guest);
                    return new ResponseEntity<>(updatedGuest, HttpStatus.OK);
                })
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteGuest(@PathVariable Integer id) {
        return guestService.getGuestById(id)
                .map(guest -> {
                    guestService.deleteGuest(id);
                    return new ResponseEntity<Void>(HttpStatus.NO_CONTENT);
                })
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @PostMapping("/find-or-create")
    public ResponseEntity<Guest> findOrCreateGuest(@RequestBody GuestCreateRequest request) {
        Guest guest = guestService.findOrCreateGuest(
            request.getFirstName(), 
            request.getLastName(), 
            request.getContactNumber());
        return new ResponseEntity<>(guest, HttpStatus.OK);
    }
    //Class for accepting data from the front end
    public static class GuestCreateRequest {
        private String firstName;
        private String lastName;
        private String contactNumber;
        
        // Getters and setters
        public String getFirstName() { return firstName; }
        public void setFirstName(String firstName) { this.firstName = firstName; }
        
        public String getLastName() { return lastName; }
        public void setLastName(String lastName) { this.lastName = lastName; }
        
        public String getContactNumber() { return contactNumber; }
        public void setContactNumber(String contactNumber) { this.contactNumber = contactNumber; }
    }
}
