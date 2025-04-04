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

	// Find or create guest - for booking process
	@Transactional
	public Guest findOrCreateGuest(String firstName, String lastName, String contactNumber) {
		// Debugging
		System.out.println("Looking for guest: firstName=" + firstName + ", lastName=" + lastName + ", contactNumber=" + contactNumber);

		// Find by Name
		List<Guest> matchNameG = guestRepository.findByFirstNameAndLastName(firstName, lastName);

		if (!matchNameG.isEmpty()) {
			System.out.println("Found " + matchNameG.size() + " guests with matching names");

			for (Guest guest : matchNameG) {
				if (contactNumber.equals(guest.getContactNumber())) {
					System.out.println("Found exact match by name and then contact: " + guest);
					return guest;
				}
			}
			System.out.println("Found name matches but contact numbers differ, creating new guest");

		}

		// If no existing guest found, create a new one
		System.out.println("Creating new guest with: firstName=" + firstName + ", lastName=" + lastName + ", contactNumber=" + contactNumber);
		Guest newGuest = new Guest(firstName, lastName, contactNumber);
		return guestRepository.save(newGuest);
	}
}
