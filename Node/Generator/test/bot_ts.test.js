'use strict';
const path = require('path');
const util = require('util');
const rimraf = require('rimraf');
const assert = require('yeoman-assert');
const helpers = require('yeoman-test');

const botName = 'sample';
const description = 'sample';

describe('Creates a bot based on Typescript', () => {
    const tmpDir = 'tmp_ts';
    const runContext = helpers.run(path.join(__dirname, '../generators/app'));
    // Setup
    beforeAll(() => {
      // The object returned acts like a promise, so return it to wait until the process is done
      return runContext.inDir(path.join(__dirname, tmpDir))
        .withPrompts({botName: botName, description: description, language: 'TypeScript'}); // Mock the prompt answers
    });
  
    afterAll((done) => {
      process.chdir(__dirname);
      runContext.cleanTestDirectory();
  
      if (runContext.completed) {
        return done();
      }
  
      runContext.on('end', done);
    });
  
    it('generates a bot app with all of the required files', () => {
      // Assertions
      assert.file([
        path.join(__dirname, `${tmpDir}/${botName}/.env`),
        path.join(__dirname, `${tmpDir}/${botName}/.gitignore`),
        path.join(__dirname, `${tmpDir}/${botName}/app.ts`),
        path.join(__dirname, `${tmpDir}/${botName}/bot.ts`),
        path.join(__dirname, `${tmpDir}/${botName}/package.json`),
        path.join(__dirname, `${tmpDir}/${botName}/README.md`)
      ]);
    });
  
    it('App file contains the correct class definition', () => {
      assert.fileContent(path.join(__dirname, `${tmpDir}/${botName}/app.ts`), 'class App');
    });
  
    it('Bot ts file calls the universalBot constructor', () => {
      assert.fileContent(path.join(__dirname, `${tmpDir}/${botName}/bot.ts`), 'const bot = new builder.UniversalBot');
    });
  
  });