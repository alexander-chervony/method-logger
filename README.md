# method logger
A way to check which of the large codebase methods are used ad which ones are probably garbage.
Intended to be used on legacy .NET projects to delete not used code. 

Steps:

1. Prepare DB in your prod/test environment by running CREATE_TABLE_AND_METHODS.sql

2. Update (add db connection string) and deploy to your environment a file to c:\temp\_methodLoggerConfig.xml 

3. Use LibWeaver to inject logger invocation to each method. The logger checks in efficient way (by just accessing array item by index) if the method was logged before or not (withing current process). If not - it just adds it to queue for logging into DB by chunks. 

The comand to update libs (by Cecil weaver):
LibWeaver.exe "c:\\sources\\theproject\\bin\\Release\\"

4. Deploy updated libraries (all content of your bin\Release including logger/facilitating libs) to your env

5. Set <Enabled>false</Enabled> in c:\temp\_methodLoggerConfig.xml if something goes wrong or performance degraded beyond your expectations :-) (should be no more than 2x worse, however check first for your application, use canary release)

6. Wait while users click the clicks or other real life app usage accurs so really used methods got logged. Go through all known actual use cases for your application.

7. Select from [LoggedMethods] to se whats going on

8. Now the fun part: within some time all code path that still alive should get logged in db .... aaaand we can delete other garbage methods and classes

Run CodeCleaner.exe to clean the code (run without params to see all options - there are quite a few including dry run and various filterings)
Probably good aproach would be to remove pieces of code on by feature/project/module basis to avoid big bang
Use good code comparer to examine code removals

9. Proceed as you wish: either delete code and let complaining users reveal the pieces that still used or whatever


Disclaimers:

- The code quite old: from 2013-2014: I didn't check whether Roslyn or Cecil advanced since then so it could be optimized or written in a better way
- It was used in production but still may have some issues (including specific to your codebase/usage), so use at your own risk and prefer canary release style with monitoring if possible
- MSSQL DB was chosen as the one that already existed in our prod env, so feel free to adapt to the one you use if it differs
- There are quite a lot todos inside, not that much of the tests. There was some controversy around this project from management side, so it didn't got into polished state
