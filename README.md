# QueryMultiDb
A command-line tool to query multiple SQL Server databases at once and store results in an excel file.

[![Build status](https://ci.appveyor.com/api/projects/status/29cusv9r5hu1r2e5?svg=true)](https://ci.appveyor.com/project/alexandre-lecoq/querymultidb)

## Command line parameters

The following parameters are supported :

Parameter|Description|Default|Required
---------|-----------|-------|--------
outputdirectory|Indicates output directory for generated file. The default is the current working directory.||
outputfile|Indicates the name of the generated file.||✔
overwrite|Overwrite output file if it already exists.|false|
targets|Indicates the list of databases to query.||
targetsstandardinput|Indicates the list of databases to query is read from standard input.|false|
targetsfile|Indicates the file containing the list of databases to query.||
query|Indicates the query to execute.||
queryfile|Indicates the file containing the SQL query to execute.||
sequential|Perform queries one at a time.|false|
connectiontimeout|The time (in seconds) to wait for a connection to open.|5|
commandtimeout|The time in seconds to wait for the command to execute.|60|
parallelism|The maximum number of queries running in parallel.|4|
startkeypress|Wait for a key press to start.|false|
stopkeypress|Wait for a key press to stop.|false|
progress|Reports progress on standard error output.|false|
nullscolor|Indicates the color of the NULL text in excel files.|7F7F7F|
shownulls|Show NULL values explicitly rather than showing empty value.|false|
showipaddress|Show server's IP address.|false|
showservername|Show server's name.|false|
showdatabasename|Show database's name.|false|
showextracolumns|Show targets' extra columns.|false|
showlogsheet|Show log sheet in excel file.|false|
showparametersheet|Show parameter sheet in excel file.|false|
showinformationmessages|Show parameter sheet in excel file.|false|
sheetlabels|Defines the sheets' labels.||
help|Display this help screen.||
version|Display version information.||

### Example

`.\QueryMultiDb.exe --progress --parallelism=8 --overwrite --queryfile="set001.sql" --outputfile="set001.xlsx" --targetsfile="set001.targets" --shownulls --showlogsheet --showparametersheet --showipaddress --showservername --showdatabasename --showinformationmessages --showextracolumns`

## Targets

The utility expects a JSON formatted file for specifying database targets.

### Simple example

```javascript
{
	"DatabaseList": [
		{ "ServerName": "localhost", "DatabaseName": "FUNNY_DB" }
	]
}
```

### Advanced example

```javascript
{
	"ExtraValue1Title": "test 1",
	"ExtraValue2Title": "test 2",
	"ExtraValue3Title": "test 3",
	"ExtraValue4Title": "test 4",
	"ExtraValue5Title": "test 5",
	"ExtraValue6Title": "test 6",
	"DatabaseList": [
		{
			"ServerName": "localhost",
			"DatabaseName": "FUNNY_DB",
			"ExtraValue1": "😸😹😺",
			"ExtraValue2": "🙈🙉🙊",
			"ExtraValue3": "😇😈😉",
			"ExtraValue4": "😇😈😉",
			"ExtraValue5": "🙍🙎🙏",
			"ExtraValue6": "😀😁😂"
		},
		{
			"ServerName": "localhost",
			"DatabaseName": "NASTY_DB",
			"ExtraValue1": "jumbo",
			"ExtraValue2": "guitar",
			"ExtraValue3": "kitchen",
			"ExtraValue4": "failure",
			"ExtraValue5": "rocknroll",
			"ExtraValue6": "beer"
		}
	]
}
```
