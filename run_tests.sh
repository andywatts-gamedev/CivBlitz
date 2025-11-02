#!/bin/bash
# Automated test runner for CI/CD

UNITY_PATH="/Applications/Unity/Hub/Editor/6000.2.7f2/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"
TEST_RESULTS="TestResults.xml"

echo "Running PlayMode tests..."
"$UNITY_PATH" \
    -runTests \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -testResults "$TEST_RESULTS" \
    -testPlatform PlayMode \
    -logFile test.log

EXIT_CODE=$?

# Show results
if [ $EXIT_CODE -eq 0 ]; then
    echo "✓ All tests passed"
else
    echo "✗ Tests failed (exit code: $EXIT_CODE)"
    echo "Check test.log for details"
fi

# Display test results if they exist
if [ -f "$TEST_RESULTS" ]; then
    echo ""
    echo "Test Results Summary:"
    grep -o 'total="[0-9]*"' "$TEST_RESULTS" || true
    grep -o 'passed="[0-9]*"' "$TEST_RESULTS" || true
    grep -o 'failed="[0-9]*"' "$TEST_RESULTS" || true
fi

exit $EXIT_CODE

