Purpose

To make scripting life easyer

Setup
Go to the appsettings.json and set the directorys to where your scripts are stored.

  appsettings:
    "RoleBackScriptLocation": "B:\\Code\\Work\\DBScripts_Rollback\\",          ***This is for roll back scripts, Should be inside the application your working on.***
    "CRUDScriptLocation": "B:\\Code\\Work\\DBScripts\\",                                                ***This is for normal scripts. Stored inside your project.***
    "ScriptDirectory": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\",          ***This folder exists outside your project, Scripts should have using at the top.***
    "RulesJsonFile": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\Configuration\\Rules.Json",        ***Path to Rules file, this file contains rules to look for.*** 
    "ScriptHistoryFile": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\Configuration\\History.txt",       ***This file stores a list of files that  have been run.***
    "SqlTemplatesFile": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\Configuration\\Templates.Json",           ***Contains the output directory and SQL templates***
    "RoleBackPrefixCharCount": 3                                                                              ***the number of digits used on the roleback scripts***

  Rules.Json:
        "TemplateName" : "CreateOrAlterProcedureNewDb",                                                                 ***Identifier to say what template to run.***
        "ScriptIdentifier": ["PROCEDURE","USE NewDb"],                                                                 ***What to look for to identify the script.*** 
        "ShouldNotContain": ["USE OtherDb"],                                                                                                      ***Exclude list.***
        "Replace": ["USE NewDb\r\nGO\r\n"]                             ***List of chars to replace. If you want to replace with a blank just wright the text to replace.
                                                                       If the text to be replaced is followed by : then the text to replace with can be added after.***

  Templates.Json:
        "Name": "PageSecurityRollbackScript",                                                                                                       ***Template Name***
		"SqlCodeTemplate": [ "USE NewDb",
							 "GO",
							 "DECLARE @ID INT = ##ReplacePageID##",
							 "DELETE FROM dbo.Table WHERE TableID = @TableID"]  ***Sql Template for Rollback***
		"ReplaceMentChars" : [ "@TableID:##ReplacePageID##"],                             ***Declare Variable Identifier and the replace charecters for the template***
		"OutputDirectory": "B:\\Code\\Work\\DBScripts\\",                                                                    ***Directory where the new script goes.***
		"ExistingCodeTemplate" : ["USE NewDb",
								  "GO"]                           ***If file exists this gets added to the top before being copied to roleback script.***

  History.txt This contains a list of files that have been run. clear the name from here if you need it to be rerun. ((todo/// Filename that gets copiedto the directory
             will come from the table/sproc name.))