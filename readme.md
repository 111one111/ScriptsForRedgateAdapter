Purpose

To make scripting life easyer

Setup
Go to the appsettings.json and set the directorys to where your scripts are stored.

  appsettings:
  ***This is for roll back scripts, Should be inside the application your working on.***
    "RoleBackScriptLocation": "B:\\Code\\Work\\DBScripts_Rollback\\",
    
    ***This is for normal scripts. Stored inside your project.***
    "CRUDScriptLocation": "B:\\Code\\Work\\DBScripts\\",
    
    ***This folder exists outside your project, Scripts should have using at the top.***
    "ScriptDirectory": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\",
    
    ***Path to Rules file, this file contains rules to look for.*** 
    "RulesJsonFile": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\Configuration\\Rules.Json",
    
    ***This file stores a list of files that  have been run.***
    "ScriptHistoryFile": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\Configuration\\History.txt",
    
     ***Contains the output directory and SQL templates***
    "SqlTemplatesFile": "B:\\Code\\ScriptsForRedgateAdapter\\Scripts\\Configuration\\Templates.Json",
    
    ***the number of digits used on the roleback scripts***
    "RoleBackPrefixCharCount": 3                                                                              

  Rules.Json:
  	***Identifier to say what template to run.***
        "TemplateName" : "CreateOrAlterProcedureNewDb",
	
	***What to look for to identify the script.*** 
        "ScriptIdentifier": ["PROCEDURE","USE NewDb"],
	
	***Exclude list.***
        "ShouldNotContain": ["USE OtherDb"],
	
	***List of chars to replace. If you want to replace with a blank just wright the text to replace.
           If the text to be replaced is followed by : then the text to replace with can be added after.***
        "Replace": ["USE NewDb\r\nGO\r\n"]                             

  Templates.Json:
  	***Template Name***
        "Name": "PageSecurityRollbackScript",                                                                                                   ***Sql Template for Rollback***
	"SqlCodeTemplateArray": [ "USE NewDb",
				 "GO",
				 "DECLARE @ID INT = ##ReplacePageID##",
				 "DELETE FROM dbo.Table WHERE TableID = @TableID"]
				 
	***Declare Variable Identifier and the replace charecters for the template***
	"ReplaceMentChars" : [ "@TableID:##ReplacePageID##"],                             
	
	***Directory where the new script goes.***
	"OutputDirectory": "B:\\Code\\Work\\DBScripts\\",
	
	***If file exists this gets added to the top before being copied to roleback script.***
	"ExistingCodeTemplateArray" : ["USE NewDb", "GO"]                           

  History.txt This contains a list of files that have been run. There has been some changes so if its a create table or sproc script it will get the name from inside the script using the name of the table its creating or the sproc.
