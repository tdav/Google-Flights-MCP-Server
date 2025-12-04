#!/bin/bash

# Example script to test Google Flights MCP Server API

# Configuration
API_BASE_URL="http://localhost:5200"

echo "==================================="
echo "Google Flights MCP Server API Tests"
echo "==================================="
echo ""

# 1. Test server info endpoint
echo "1. Testing Server Info Endpoint..."
echo "   GET /"
curl -s "${API_BASE_URL}/" | jq .
echo ""
echo ""

# 2. Test simple one-way flight search
echo "2. Testing One-Way Flight Search..."
echo "   Route: New York (JFK) to Los Angeles (LAX)"
echo "   Date: 2024-12-15"
echo "   Passengers: 1"
echo "   Class: Economy"
curl -s "${API_BASE_URL}/api/flights/search?origin=JFK&destination=LAX&departureDate=2024-12-15&passengers=1&cabinClass=economy" | jq .
echo ""
echo ""

# 3. Test round-trip flight search
echo "3. Testing Round-Trip Flight Search..."
echo "   Route: San Francisco (SFO) to New York City (NYC)"
echo "   Departure: 2024-12-20"
echo "   Return: 2024-12-27"
echo "   Passengers: 2"
echo "   Class: Business"
curl -s "${API_BASE_URL}/api/flights/search?origin=SFO&destination=NYC&departureDate=2024-12-20&returnDate=2024-12-27&passengers=2&cabinClass=business" | jq .
echo ""
echo ""

# 4. Test MCP endpoint with POST request
echo "4. Testing MCP Search Endpoint (POST)..."
echo "   Route: London (LHR) to Tokyo (TYO)"
echo "   Date: 2024-12-25"
echo "   Passengers: 1"
echo "   Class: First"
curl -s -X POST "${API_BASE_URL}/api/mcp/search" \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "LHR",
    "destination": "TYO",
    "departureDate": "2024-12-25",
    "passengers": 1,
    "cabinClass": "first"
  }' | jq .
echo ""
echo ""

# 5. Test error handling - invalid cabin class
echo "5. Testing Error Handling (Invalid Cabin Class)..."
curl -s "${API_BASE_URL}/api/flights/search?origin=JFK&destination=LAX&departureDate=2024-12-15&passengers=1&cabinClass=luxury" | jq .
echo ""
echo ""

# 6. Test error handling - invalid passenger count
echo "6. Testing Error Handling (Invalid Passenger Count)..."
curl -s "${API_BASE_URL}/api/flights/search?origin=JFK&destination=LAX&departureDate=2024-12-15&passengers=15&cabinClass=economy" | jq .
echo ""
echo ""

echo "==================================="
echo "All tests completed!"
echo "==================================="
