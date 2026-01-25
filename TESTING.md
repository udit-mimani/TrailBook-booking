# GoTyolo API Testing Documentation

## Test Scenarios

### ✅ Happy Path - Complete Booking Flow
### ✅ Cancellation Path - Refund Scenarios
### ✅ Edge Cases & Error Handling
### ✅ Concurrency Testing
### ✅ Background Job Verification

---
## Test Results

## Happy Path

### 1. SETUP - Health Check
- Status: ```200 OK```
- Body: Should show array of trips (might be empty)

### 2. Create Trip
- Status: ```201 Created```
- Body contains ```id```, ```title```, ```maxCapacity: 10```, ```availableSeats: 10```
- The ```trip_id``` variable is now populated!

### 3. Get Trip Details
- Status: ```200 OK```
- Response shows trip detils

### 4. Create Booking
- Status: ```200 OK```
- ```state```: "PendingPayment"
- ```priceAtBooking```: 200
- ```expiresAt```: future timestamp
- The ```booking_id``` variable is not populated

### 5. Check Booking status (Before payment)
- Status: ```200 OK```
- ```state```: 0 (PendingPayment enum value)

### 6. Payment - Webhook Success
- Status: ```200 OK```

### 7. Verify - Get Booking (After Payment)
- Status: ```200 OK```
- ```state```: 1 (Confirmed)
- ```paymentReference```: not empty

### 8. IDEMPOTENCY - Duplicate Webhook
- Status: ```200 OK``` (same idempotency key)
 
## Cancellation Flow

### 9. CANCEL - Create Booking for Cancellation
- Status: ```200 OK```

### 10. CANCEL - Confirm Payment
- Status: ```200 OK```

### 11. CANCEL - Cancel Booking
- Status: ```200 OK```
- Verify Refund: ```270``` (300*0.9)

## Admin Endpoints

### 12. ADMIN - Get Trip Metrics
- Status: ```200 OK```
- ```occupancyPercent```: 20.0
- ```netRevenue```: 230.00

### 13. ADMIN - Get At-Risk Trips
- Status: ```200 OK```
- ```occupancyPercent```: 0

## Error Handling

### 14. ERROR - Book Non-Existent Trip
- Status: ```409 Conflict```
- ```error```: ```Trip not found```

### 15. ERROR - Book Too Many Seats
- Status: ```409 Conflict```
- ```error```: ```Cannot book 100 Seats. Available: 8```
