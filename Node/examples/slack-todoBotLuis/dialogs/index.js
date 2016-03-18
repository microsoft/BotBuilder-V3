var builder = require('../../../');
var prompts = require('../prompts');

/** Return a LuisDialog that points at our model and then add intent handlers. */
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=0748d3d3-eb60-4e58-95df-3d5e904c5fdc&subscription-key=fe054e042fd14754a83f0a205f6552a5&q=');
module.exports = dialog;

/** Answer users help requests. We can use a DialogAction to send a static message. */
dialog.on('Help', builder.DialogAction.send(prompts.helpMessage));

/** Prompts a user for the title of the task and saves it.  */
dialog.on('SaveTask', [
    function (session, args, next) {
        // See if got the tasks title from our LUIS model.
        var title = builder.EntityRecognizer.findEntity(args.entities, 'TaskTitle');
        if (!title) {
            // Prompt user to enter title.
            builder.Prompts.text(session, prompts.saveTaskMissing);    
        } else {
            // Pass title to next step.
            next({ response: title.entity });
        }
    },
    function (session, results) {
        // Save the task
        if (results.response) {
            if (!session.channelData.tasks) {
                session.channelData.tasks = [results.response];
            } else {
                session.channelData.tasks.push(results.response);
            }
            session.send(prompts.saveTaskCreated, { task: results.response });
        } else {
            session.send(prompts.canceled);
        }
    }
]);

/** Prompts the user for the task to delete and then removes it. */
dialog.on('FinishTask', [
    function (session, args, next) {
        // Do we have any tasks?
        if (session.channelData.tasks && session.channelData.tasks.length > 0) {
            // See if got the tasks title from our LUIS model.
            var topTask;
            var title = builder.EntityRecognizer.findEntity(args.entities, 'TaskTitle');
            if (title) {
                // Find it in our list of tasks
                topTask = builder.EntityRecognizer.findBestMatch(session.channelData.tasks, title.entity);
            }
            
            // Prompt user if task missing or not found
            if (!topTask) {
                builder.Prompts.choice(session, prompts.finishTaskMissing, session.channelData.tasks);
            } else {
                next({ response: topTask });
            }
        } else {
            session.send(prompts.listNoTasks);
        }
    },
    function (session, results) {
        if (results && results.response) {
            session.channelData.tasks.splice(results.response.index, 1);
            session.send(prompts.finishTaskDone, { task: results.response.entity });
        } else {
            session.send(prompts.canceled);
        }
    }
]);

/** Shows the user a list of tasks. */
dialog.on('ListTasks', function (session) {
    if (session.channelData.tasks && session.channelData.tasks.length > 0) {
        var list = '';
        session.channelData.tasks.forEach(function (value, index) {
            list += session.gettext(prompts.listTaskItem, { index: index + 1, task: value });
        });
        session.send(prompts.listTaskList, list);
    }
    else {
        session.send(prompts.listNoTasks);
    }
});
