module.exports = {
    unknown: "I didn't understand. Type '/help' for a list of commands.",
    helpMessage: 'Available commands are:\n' +
    '* /list - shows a list of supported tests.\n' +
    '* /run [test] - runs a test.\n' +
    '* /run-async [test] - runs a test asynchronously.\n' +
    '* /quit - ends the conversation with TestBot.',
    testListItem: '\n* %(name)s - %(description)s',
    testList: 'List of supported tests:%s',
    runPrompt: 'Which test would you like to run?',
    runAsyncTest: "Running the '%s' test in 4 seconds...",
    testCompleted: "The '%s' test has finished.",
    canceled: 'Ok...',
    goodbye: 'Goodbye...'
};
