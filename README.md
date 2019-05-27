# ecommerceimport

This is an ETL job for processing verified transactions from a bank source and updating a custom invoice database to reflect these payments.  It allows fetching of remittance files from multiple remote sources as well as 

This was a project where I got to take my time a bit more and put more care into the organization of the code.  This was an upgrade from a process that was chiefly being done through MS SQL SSIS.  That former project was a labyrinthine tangle of complex workflows, batch file calls, and redundant/irrelevant processes.  Converting it into C# console project gave me the opportunity to introduce some things:

<ul>
<li>
Entity Framework - using an ORM helped me think less about interacting with the database and more about how to manipulate the data
</li>
<li>
Logging - I injected a logging object into the various processes to have a historical record of what happened each time the job ran
</li>

<li>
Tests!  With the pace of the work that's required of me, I don't often get time to write test coverage of any type.  But with this project I was able to put in some key unit tests, found in a separate repo at https://github.com/JeTmAn1981/ecommerceimporttests.
</li>

<li>
Proper abstractions - using abstract classes I was able to avoid a lot of code duplication and create reusable classes that could be effective in multiple places not just in this project but in other ETL projects that I have subsequently written.
</li>
</ul>

Overall I feel good about what I was able to accomplish with this project.  It works well, and when it doesn't work it fails fairly gracefully, and it provides tools which serve as natural extensions to other projects.
