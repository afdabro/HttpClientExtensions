#Cloud Computing and Big Data using Microservices

As developers, we are constantly learning in order to solve more complex problems. Whether it is a new framework, language, design pattern, or process. I was recently given the following problem to solve:

A business has billions of CSV files in a data storage container. The CSV records are unstructured. The value labels are not guaranteed (ie. A label might contain camel case, underscores, etc.). They would like to expose the data to other services easily, provide real-time streaming, and query the data set with dynamic mapping.

At the time, I did not know specific technologies that would solve the problem. My expertise is with building Azure cloud services from scratch and connecting IoT devices to the cloud. I have a firm understanding on how microservices work and I enjoy a good challenge.

I designed the following high level architecture:

An import worker process would read files from the CSV storage container, extract the records, perform any transformations (in this case they would like the RAW data stored), and load the records into a NoSQL database. If a file failed during the ETL process, the errors would be logged and the file moved to an error storage container. In order to enable dynamic querying, another microservice would provide custom label mapping functionality.
The streaming portion is slightly different. Messages are sent to an event hub which are picked up by an event processor. This processor delegates to a chain of tasks. Each task accepts input in a specific format and may produce output. The tasks are individually scalable based on the number of worker roles. One of the tasks would store the real-time streaming data into the NoSQL storage.

I was slightly curious after the discussion to see if my assumptions on big data architecture were correct. I googled for an hour and found three technologies which provide the necessary core functionality out of the box.

[HBase](https://hbase.apache.org/) is a very prominent NoSQL database which supports high volume, data variety, and a decent frequency. Like most NoSQL databases HBase is deployed as a cluster. You might ask, "How do you connect to HBase?" Surprisingly, through a RESTful web service! Continuing with the above business scenario... The import worker role would read files from the CSV storage container, extract the rows, transform the data using the HBase Mapper, and load the data with a MapReduce job.

*Note:* In order to scale the import worker role you would need to decouple reading the collection of files. A better solution would have a file "picker" which passes an event message to the import worker roles with the necessary information for accessing the file. Thus, it is possible to scale the worker roles without worrying about processing a file twice. You may have also noticed that the import worker role is breaking the "do one thing" microservice rule by performing a complete ETL pipeline on a single instance. In a long running operation scenario or in an Agile environment this could be "good enough." Another option is to split each ETL task into its own worker process service. Storm provides this functionality in a cluster environment. More on Storm in a bit.

Now what about the dynamic query mapping? [Hive](https://hive.apache.org/) is a query engine built for HBase. It provides out of the box SQL-like capabilities including mapping and exposes... you might have guessed a RESTful interface. Hive also has a few other options for user interactions (ie. ODBC, Web UI).

I mentioned [Storm](http://storm.apache.org/) earlier. Storm is an enabler of real-time event streaming. If you are familiar with [C# Task Parallel Library's Dataflow](https://msdn.microsoft.com/en-us/library/hh228603%28v=vs.110%29.aspx) the concepts are very similar. Following the business use case... Event messages are added to an event messaging hub. The event processor, in Storm terminology would be a spout, ingests messages. They are piped into a series of tasks known as bolts. Each bolt may be scaled separately. A Storm cluster consists of supervisors (which execute the worker processes of spout or bolts), a job tracker (called a Nimbus), and a zookeeper (similar to a router).


Azure released HBase, Hive, and Storm as a managed set of services [HDInsight](https://azure.microsoft.com/en-us/services/hdinsight/).

Make life easy. Eat Code for Breakfast.



