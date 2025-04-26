package com.steebo.booking.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import com.steebo.booking.model.Booking;
import com.steebo.booking.model.Guest;
import com.steebo.booking.model.Room;
import com.steebo.booking.service.BookingService;
import com.steebo.booking.service.GuestService;
import com.steebo.booking.service.RoomService;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Optional;

@RestController
@RequestMapping("/api/bookings")
@CrossOrigin(origins = "*") // For development - restrict in production
public class BookingController {

    private final BookingService bookingService;
    private final GuestService guestService;
    private final RoomService roomService;

    @Autowired
    public BookingController(BookingService bookingService, GuestService guestService, RoomService roomService) {
        this.bookingService = bookingService;
        this.guestService = guestService;
        this.roomService = roomService;
        
        System.out.println("Booking Controller Running");
    }

    @GetMapping
    public ResponseEntity<List<Booking>> getAllBookings() {
        List<Booking> bookings = bookingService.getAllBookings();
        return new ResponseEntity<>(bookings, HttpStatus.OK);
    }

    @GetMapping("/{id}")
    public ResponseEntity<Booking> getBookingById(@PathVariable Integer id) {
        return bookingService.getBookingById(id)
                .map(booking -> new ResponseEntity<>(booking, HttpStatus.OK))
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/guest/{guestId}")
    public ResponseEntity<List<Booking>> getBookingsByGuest(@PathVariable Integer guestId) {
        return guestService.getGuestById(guestId)
                .map(guest -> {
                    List<Booking> bookings = bookingService.getBookingsByGuest(guest);
                    return new ResponseEntity<>(bookings, HttpStatus.OK);
                })
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/room/{roomId}")
    public ResponseEntity<List<Booking>> getBookingsByRoom(@PathVariable Integer roomId) {
        return roomService.getRoomById(roomId)
                .map(room -> {
                    List<Booking> bookings = bookingService.getBookingsByRoom(room);
                    return new ResponseEntity<>(bookings, HttpStatus.OK);
                })
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @GetMapping("/status/{status}")
    public ResponseEntity<List<Booking>> getBookingsByStatus(@PathVariable String status) {
        try {
            Booking.BookingStatus bookingStatus = Booking.BookingStatus.valueOf(status);
            List<Booking> bookings = bookingService.getBookingsByStatus(bookingStatus);
            return new ResponseEntity<>(bookings, HttpStatus.OK);
        } catch (IllegalArgumentException e) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }
    }
    
	@GetMapping("/search")
	public ResponseEntity<List<Booking>> searchBookings(@RequestParam(required = false) String guestName,
			@RequestParam(required = false) String contactNumber, @RequestParam(required = false) String roomNumber) {

		List<Booking> results = new ArrayList<>();

		if (guestName != null && !guestName.isEmpty()) {
			// Search by guest name
			results = bookingService.findBookingsByGuestName(guestName);
		} else if (contactNumber != null && !contactNumber.isEmpty()) {
			// Search by contact number
			results = bookingService.findBookingsByContactNumber(contactNumber);
		} else if (roomNumber != null && !roomNumber.isEmpty()) {
			// Search by room number
			results = bookingService.findBookingsByRoomNumber(roomNumber);
		}

		return new ResponseEntity<>(results, HttpStatus.OK);
	}
	
	@GetMapping("/date-range")
	public ResponseEntity<List<Booking>> getBookingsInDateRange(
	        @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) Date fromDate,
	        @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) Date toDate) {
	    
	    List<Booking> bookings = bookingService.getBookingsForDateRange(fromDate, toDate);
	    return new ResponseEntity<>(bookings, HttpStatus.OK);
	}

    @PostMapping
    public ResponseEntity<Booking> createBooking(@RequestBody BookingRequest request) {
        try {
        	System.out.println("Recived booking request: " + request);
        	System.out.println("Guest ID: " + request.getGuestId());
        	System.out.println("Room ID: " + request.getRoomId());
        	System.out.println("Check-In Date: " + request.getCheckInDate());
        	System.out.println("Check-out date: " + request.getCheckOutDate());
        	
            // Get guest and room
            Optional<Guest> guestOpt = guestService.getGuestById(request.getGuestId());
            Optional<Room> roomOpt = roomService.getRoomById(request.getRoomId());
            
            if (guestOpt.isEmpty() || roomOpt.isEmpty()) {
                return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
            }
            
            Guest guest = guestOpt.get();
            Room room = roomOpt.get();
            
            // Check if room is available for the requested dates
            if (!roomService.isRoomAvailable(room.getRoomId(), request.getCheckInDate(), request.getCheckOutDate())) {
                return new ResponseEntity<>(HttpStatus.CONFLICT);
            }
            
            // Create and populate booking object
            Booking booking = new Booking(guest, room, request.getCheckInDate(), request.getCheckOutDate());
            booking.setBreakfastIncluded(request.isBreakfastIncluded());
            booking.setNotes(request.getNotes());
            
            // Calculate total price based on number of nights
            long nights = (request.getCheckOutDate().getTime() - request.getCheckInDate().getTime()) / (1000 * 60 * 60 * 24);
            int totalPrice = room.getPricePerNight() * (int)nights;
            booking.setTotalPrice(totalPrice);
            
            // Save booking
            Booking savedBooking = bookingService.saveBooking(booking);
            
            // Update room status
            room.setStatus(Room.Status.occupied);
            roomService.saveRoom(room);
            
            return new ResponseEntity<>(savedBooking, HttpStatus.CREATED);
        } 
        
        catch (Exception e) 
        {
            return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }

    @PutMapping("/{id}")
    public ResponseEntity<Booking> updateBooking(@PathVariable Integer id, @RequestBody BookingUpdateRequest request) {
        return bookingService.getBookingById(id)
                .map(existingBooking -> {
                	existingBooking.setCheckInDate(request.getCheckInDate());
                    existingBooking.setCheckOutDate(request.getCheckOutDate());
                    existingBooking.setBreakfastIncluded(request.isBreakfastIncluded());
                    existingBooking.setNotes(request.getNotes());
                    
                    // Apply updated prices
                    Room room = existingBooking.getRoom();
                    long nights = (request.getCheckOutDate().getTime() - request.getCheckInDate().getTime()) / (1000 * 60 * 60 * 24);
                    int totalPrice = room.getPricePerNight() * (int)nights;
                    existingBooking.setTotalPrice(totalPrice);
                    
                    // Save the updated booking to the database
                    Booking updatedBooking = bookingService.saveBooking(existingBooking);
                    return new ResponseEntity<>(updatedBooking, HttpStatus.OK);
                })
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }

    @PutMapping("/{id}/status")
    public ResponseEntity<Booking> updateBookingStatus(
            @PathVariable Integer id, 
            @RequestParam String status) {
        try {
            Booking.BookingStatus bookingStatus = Booking.BookingStatus.valueOf(status);
            
            return bookingService.getBookingById(id)
                    .map(booking -> {
                        booking.setBookingStatus(bookingStatus);
                        
                        // Handle room status update
                        Room room = booking.getRoom();
                        if (bookingStatus == Booking.BookingStatus.checked_out || 
                            bookingStatus == Booking.BookingStatus.cancelled) {
                            room.setStatus(Room.Status.available);
                            roomService.saveRoom(room);
                        } else if (bookingStatus == Booking.BookingStatus.checked_in) {
                            room.setStatus(Room.Status.occupied);
                            roomService.saveRoom(room);
                        }
                        
                        Booking updatedBooking = bookingService.saveBooking(booking);
                        return new ResponseEntity<>(updatedBooking, HttpStatus.OK);
                    })
                    .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
        } catch (IllegalArgumentException e) {
            return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
        }
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteBooking(@PathVariable Integer id) {
        return bookingService.getBookingById(id)
                .map(booking -> {
                    // Free up the room if needed
                    Room room = booking.getRoom();
                    room.setStatus(Room.Status.available);
                    roomService.saveRoom(room);
                    
                    bookingService.deleteBooking(id);
                    return new ResponseEntity<Void>(HttpStatus.NO_CONTENT);
                })
                .orElse(new ResponseEntity<>(HttpStatus.NOT_FOUND));
    }
    
    // Inner class for booking request
    public static class BookingRequest {
        private Integer guestId;
        private Integer roomId;
        
        @DateTimeFormat(iso = DateTimeFormat.ISO.DATE)
        private Date checkInDate;
        
        @DateTimeFormat(iso = DateTimeFormat.ISO.DATE)
        private Date checkOutDate;
        
        private boolean breakfastIncluded;
        private String notes;
        
        // Getters and setters
        public Integer getGuestId() {
            return guestId;
        }
        
        public void setGuestId(Integer guestId) {
            this.guestId = guestId;
        }
        
        public Integer getRoomId() {
            return roomId;
        }
        
        public void setRoomId(Integer roomId) {
            this.roomId = roomId;
        }
        
        public Date getCheckInDate() {
            return checkInDate;
        }
        
        public void setCheckInDate(Date checkInDate) {
            this.checkInDate = checkInDate;
        }
        
        public Date getCheckOutDate() {
            return checkOutDate;
        }
        
        public void setCheckOutDate(Date checkOutDate) {
            this.checkOutDate = checkOutDate;
        }
        
        public boolean isBreakfastIncluded() {
            return breakfastIncluded;
        }
        
        public void setBreakfastIncluded(boolean breakfastIncluded) {
            this.breakfastIncluded = breakfastIncluded;
        }
        
        public String getNotes() {
            return notes;
        }
        
        public void setNotes(String notes) {
            this.notes = notes;
        }
        
        @Override
        public String toString() {
        	return "BookingRequest{" +
                    "guestId=" + guestId +
                    ", roomId=" + roomId +
                    ", checkInDate=" + checkInDate +
                    ", checkOutDate=" + checkOutDate +
                    ", breakfastIncluded=" + breakfastIncluded +
                    ", notes='" + notes + '\'' +
                    '}';
        }
    }
    
    // Inner class for booking update request
    public static class BookingUpdateRequest {
        @DateTimeFormat(iso = DateTimeFormat.ISO.DATE)
        private Date checkInDate;
        
        @DateTimeFormat(iso = DateTimeFormat.ISO.DATE)
        private Date checkOutDate;
        
        private boolean breakfastIncluded;
        private String notes;
        
        // Getters and setters
        public Date getCheckInDate() {
            return checkInDate;
        }
        
        public void setCheckInDate(Date checkInDate) {
            this.checkInDate = checkInDate;
        }
        
        public Date getCheckOutDate() {
            return checkOutDate;
        }
        
        public void setCheckOutDate(Date checkOutDate) {
            this.checkOutDate = checkOutDate;
        }
        
        public boolean isBreakfastIncluded() {
            return breakfastIncluded;
        }
        
        public void setBreakfastIncluded(boolean breakfastIncluded) {
            this.breakfastIncluded = breakfastIncluded;
        }
        
        public String getNotes() {
            return notes;
        }
    }
}