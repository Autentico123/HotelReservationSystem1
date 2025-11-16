# GCash QR Code Setup

## How to Add Your GCash QR Code

1. **Generate your GCash QR Code:**
   - Open your GCash app
   - Tap on "QR Code" or "Receive Money"
   - Save/Screenshot your QR code

2. **Add the QR code to your project:**
   - Save your QR code image as `gcash-qr.png` or `gcash-qr.jpg`
   - Place it in: `wwwroot/images/gcash-qr.png`

3. **Create the images folder if it doesn't exist:**
   ```
   HotelReservationSystem1/
   ??? wwwroot/
       ??? images/
           ??? gcash-qr.png  (your QR code image here)
   ```

## Alternative: Use a Placeholder

If you don't have a QR code yet, the system will show:
- Hotel's GCash number: **09123456789** (update this in the view)
- A placeholder message to scan the QR code

## Update Your GCash Number

Edit `Views/Payments/GCashPayment.cshtml` and update:
- Line with GCash number: Change `09123456789` to your actual number
- QR code image path if different

## QR Code Specifications

- **Format**: PNG or JPG
- **Recommended Size**: 300x300 pixels minimum
- **Max Size**: 1MB for faster loading
- **Quality**: Clear and scannable

## Testing

1. Upload your QR code image
2. Navigate to any reservation
3. Click "Pay with GCash"
4. Verify the QR code displays correctly
5. Test scanning with your GCash app
