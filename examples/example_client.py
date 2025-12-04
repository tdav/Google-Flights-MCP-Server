#!/usr/bin/env python3
"""
Example Python client for Google Flights MCP Server

This script demonstrates how to interact with the Google Flights MCP Server API
using Python's requests library.
"""

import requests
import json
from datetime import datetime, timedelta

# Configuration
API_BASE_URL = "http://localhost:5200"


def get_server_info():
    """Get server information"""
    response = requests.get(f"{API_BASE_URL}/")
    return response.json()


def search_one_way_flight(origin, destination, departure_date, passengers=1, cabin_class="economy"):
    """Search for one-way flights"""
    params = {
        "origin": origin,
        "destination": destination,
        "departureDate": departure_date,
        "passengers": passengers,
        "cabinClass": cabin_class
    }
    response = requests.get(f"{API_BASE_URL}/api/flights/search", params=params)
    return response.json()


def search_round_trip_flight(origin, destination, departure_date, return_date, passengers=1, cabin_class="economy"):
    """Search for round-trip flights"""
    params = {
        "origin": origin,
        "destination": destination,
        "departureDate": departure_date,
        "returnDate": return_date,
        "passengers": passengers,
        "cabinClass": cabin_class
    }
    response = requests.get(f"{API_BASE_URL}/api/flights/search", params=params)
    return response.json()


def search_mcp(origin, destination, departure_date, return_date=None, passengers=1, cabin_class="economy"):
    """Search using MCP endpoint"""
    payload = {
        "origin": origin,
        "destination": destination,
        "departureDate": departure_date,
        "returnDate": return_date,
        "passengers": passengers,
        "cabinClass": cabin_class
    }
    response = requests.post(f"{API_BASE_URL}/api/mcp/search", json=payload)
    return response.json()


def print_flight_results(results):
    """Pretty print flight search results"""
    print(f"\n{'='*80}")
    print(f"Flight Search Results: {results['origin']} → {results['destination']}")
    print(f"Departure: {results['departureDate']}")
    if results.get('returnDate'):
        print(f"Return: {results['returnDate']}")
    print(f"Passengers: {results['passengers']} | Class: {results['cabinClass'].replace('_', ' ').title()}")
    print(f"{'='*80}\n")
    
    for i, flight in enumerate(results['flights'], 1):
        print(f"{i}. {flight['airline']} - {flight['flightNumber']}")
        print(f"   Departure: {flight['departureTime']} | Arrival: {flight['arrivalTime']}")
        print(f"   Duration: {flight['duration']} | Stops: {flight['stops']}")
        print(f"   Price: {flight['currency']} ${flight['price']:.2f}")
        print()
    
    print(f"Google Flights URL: {results['searchUrl']}")
    print(f"{'='*80}\n")


def main():
    """Main example function"""
    print("Google Flights MCP Server - Python Client Example")
    print("=" * 80)
    
    try:
        # 1. Get server info
        print("\n1. Getting server information...")
        server_info = get_server_info()
        print(f"   Server: {server_info['name']}")
        print(f"   Version: {server_info['version']}")
        print(f"   Description: {server_info['description']}")
        
        # 2. Search for one-way flight
        print("\n2. Searching for one-way flight (JFK → LAX)...")
        tomorrow = (datetime.now() + timedelta(days=1)).strftime("%Y-%m-%d")
        one_way = search_one_way_flight("JFK", "LAX", tomorrow, passengers=1, cabin_class="economy")
        print_flight_results(one_way)
        
        # 3. Search for round-trip flight
        print("\n3. Searching for round-trip flight (SFO ⇄ NYC)...")
        departure = (datetime.now() + timedelta(days=7)).strftime("%Y-%m-%d")
        return_date = (datetime.now() + timedelta(days=14)).strftime("%Y-%m-%d")
        round_trip = search_round_trip_flight("SFO", "NYC", departure, return_date, passengers=2, cabin_class="business")
        print_flight_results(round_trip)
        
        # 4. Search using MCP endpoint
        print("\n4. Searching using MCP endpoint (LHR → TYO)...")
        departure = (datetime.now() + timedelta(days=30)).strftime("%Y-%m-%d")
        mcp_result = search_mcp("LHR", "TYO", departure, passengers=1, cabin_class="first")
        print(f"MCP Tool: {mcp_result['tool']}")
        print_flight_results(mcp_result['content'])
        
        print("\n✅ All examples completed successfully!")
        
    except requests.exceptions.ConnectionError:
        print("\n❌ Error: Could not connect to the server.")
        print("   Make sure the server is running on http://localhost:5200")
    except Exception as e:
        print(f"\n❌ Error: {e}")


if __name__ == "__main__":
    main()
