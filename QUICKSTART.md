# Quick Start Guide

## Prerequisites

- ✅ .NET 8.0 SDK installed
- ✅ SQL Server or SQL Server Express installed
- ✅ SQL Server running

## Setup (First Time)

### Option 1: Automated Setup

Run the setup script:

```bash
setup.bat
```

This will:
1. Check .NET SDK
2. Restore NuGet packages
3. Build the project
4. Create uploads directory
5. Apply database migrations

### Option 2: Manual Setup

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Create uploads directory:**
   ```bash
   mkdir -p wwwroot/uploads/wastage
   ```

4. **Apply database migrations:**
   ```bash
   dotnet ef database update
   ```

## Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTPS**: https://localhost:7xxx
- **Swagger UI**: https://localhost:7xxx/swagger

## Testing the API

### Using Swagger UI

1. Navigate to https://localhost:7xxx/swagger
2. Expand the `/api/wastage` POST endpoint
3. Click "Try it out"
4. Fill in the form data:
   - InwardChallanId: "IC001"
   - PartyName: "Test Company"
   - VehicleNo: "MH-12-AB-1234"
   - Date: Current date
   - NetWeight: 1000
   - MouReport: [10.5, 15.75]
   - ImageFiles: Upload 1-3 images
5. Click "Execute"

### Using Postman

1. Create a new POST request to: `https://localhost:7xxx/api/wastage`
2. Set Body type to `form-data`
3. Add fields:
   ```
   InwardChallanId: IC001
   PartyName: ABC Company
   VehicleNo: MH-12-AB-1234
   SlipNo: SL001
   Date: 2025-10-10T10:00:00
   NetWeight: 1000.50
   MouReport: 10.5
   MouReport: 15.75
   MouReport: 20.25
   ImageFiles: [Choose files]
   ```
4. Send the request

### Using curl

```bash
curl -X POST "https://localhost:7xxx/api/wastage" \
  -H "accept: */*" \
  -H "Content-Type: multipart/form-data" \
  -F "InwardChallanId=IC001" \
  -F "PartyName=ABC Company" \
  -F "VehicleNo=MH-12-AB-1234" \
  -F "SlipNo=SL001" \
  -F "Date=2025-10-10T10:00:00" \
  -F "NetWeight=1000.50" \
  -F "MouReport=10.5" \
  -F "MouReport=15.75" \
  -F "ImageFiles=@image1.jpg" \
  -F "ImageFiles=@image2.jpg"
```

## Verify Database

```sql
-- Connect to SQL Server
USE JumboRollDB;

-- Check tables
SELECT * FROM Wastages;

-- Check specific entry
SELECT * FROM Wastages WHERE InwardChallanId = 'IC001';
```

## Frontend Integration

Update your frontend `.env` file:

```env
NEXT_PUBLIC_DOTNET_URL=https://localhost:7xxx/api
```

The frontend will automatically use this URL for API calls.

## Common Issues

### Issue: Database connection failed

**Solution**: Check SQL Server is running and update connection string in `appsettings.json`

### Issue: CORS error from frontend

**Solution**: Add your frontend URL to CORS policy in `Program.cs`:

```csharp
policy.WithOrigins("http://localhost:3000", "YOUR_FRONTEND_URL")
```

### Issue: Images not displaying

**Solution**: Check that:
- Images were uploaded successfully
- wwwroot/uploads/wastage directory exists
- Static files middleware is enabled (already configured)

### Issue: Port already in use

**Solution**: Change port in `Properties/launchSettings.json` or stop the other application

## Next Steps

1. Test all API endpoints in Swagger UI
2. Integrate with your frontend
3. Configure production database connection
4. Set up proper authentication/authorization (if needed)
5. Deploy to production server

## Support

For issues or questions:
- Check the README.md for detailed documentation
- Review Swagger UI for API details
- Check logs in console output
