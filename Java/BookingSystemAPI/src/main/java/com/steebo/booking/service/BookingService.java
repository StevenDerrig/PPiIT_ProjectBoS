package com.steebo.booking.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.steebo.booking.model.Booking;
import com.steebo.booking.model.Guest;
import com.steebo.booking.model.Room;
import com.steebo.booking.repository.BookingRepo;

import java.util.Date;
import java.util.List;
import java.util.Optional;

@Service
public class BookingService {

    private final BookingRepo bookingRepository;

    @Autowired
    public BookingService(BookingRepo bookingRepository) {
        this.bookingRepository = bookingRepository;
    }

    public List<Booking> getAllBookings() {
        return bookingRepository.findAll();
    }

    public Optional<Booking> getBookingById(Integer id) {
        return bookingRepository.findById(id);
    }

    public List<Booking> getBookingsByGuest(Guest guest) {
        return bookingRepository.findByGuest(guest);
    }

    public List<Booking> getBookingsByRoom(Room room) {
        return bookingRepository.findByRoom(room);
    }

    public List<Booking> getBookingsByStatus(Booking.BookingStatus status) {
        return bookingRepository.findByBookingStatus(status);
    }

    public List<Booking> getBookingsForDateRange(Date checkInDate, Date checkOutDate) {
        return bookingRepository.findBookingsInDateRange(checkInDate, checkOutDate);
    }

    public List<Booking> getTodayCheckIns() {
        return bookingRepository.findTodayCheckIns();
    }

    public List<Booking> getTodayCheckOuts() {
        return bookingRepository.findTodayCheckOuts();
    }

    @Transactional
    public Booking saveBooking(Booking booking) {
        // If it's a new booking, set the created date
        if (booking.getCreatedAt() == null) {
            booking.setCreatedAt(new Date());
        }
        
        // If no booking status is set, default to confirmed
        if (booking.getBookingStatus() == null) {
            booking.setBookingStatus(Booking.BookingStatus.confirmed);
        }
        
        return bookingRepository.save(booking);
    }

    @Transactional
    public void deleteBooking(Integer id) {
        bookingRepository.deleteById(id);
    }

    // Cancel a booking
    @Transactional
    public Booking cancelBooking(Integer id) {
        Optional<Booking> bookingOpt = bookingRepository.findById(id);
        
        if (bookingOpt.isPresent()) {
            Booking booking = bookingOpt.get();
            booking.setBookingStatus(Booking.BookingStatus.cancelled);
            return bookingRepository.save(booking);
        }
        
        return null;
    }

    // Check in a guest
    @Transactional
    public Booking checkInGuest(Integer id) {
        Optional<Booking> bookingOpt = bookingRepository.findById(id);
        
        if (bookingOpt.isPresent()) {
            Booking booking = bookingOpt.get();
            booking.setBookingStatus(Booking.BookingStatus.checked_in);
            return bookingRepository.save(booking);
        }
        
        return null;
    }

    // Check out a guest
    @Transactional
    public Booking checkOutGuest(Integer id) {
        Optional<Booking> bookingOpt = bookingRepository.findById(id);
        
        if (bookingOpt.isPresent()) {
            Booking booking = bookingOpt.get();
            booking.setBookingStatus(Booking.BookingStatus.checked_out);
            return bookingRepository.save(booking);
        }
        
        return null;
    }
}