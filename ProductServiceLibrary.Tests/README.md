# ProductServiceLibrary Tests

This test project contains unit tests for the ProductServiceLibrary models.

## Test Framework

- **Framework**: xUnit
- **Target**: .NET 10.0

## Running Tests

To run all tests:

```bash
cd ProductServiceLibrary.Tests
dotnet test
```

To run tests with detailed output:

```bash
dotnet test --verbosity normal
```

## Test Coverage

### ProductModelTests
- Tests for Product model properties
- Tests for price calculations
- Tests for inventory tracking
- Tests for optional properties (LastModifiedDate)

### CategoryModelTests
- Tests for Category model properties
- Tests for multiple categories
- Tests for optional descriptions

## Test Results

All tests are passing:
- Total tests: 13
- Passed: 13
- Failed: 0
- Skipped: 0

## Notes

This is a standalone test project created for testing purposes. The Product and Category models are copied into this project to enable testing without requiring the full .NET Framework WCF build environment.
