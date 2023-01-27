# didehpc command line tool

This repo is C# code that compiles to an executable `didehpc.exe`. It provides
a number of utility calls for working with an on-premises Microsoft Azure Cluster
running MS HPC Pack. The calls are sometimes faster than the MS `job` client, 
sometimes by combining multiple `job` calls into one keeping the session open 
with the scheduler, avoiding restart overheads.

It is an admin tool, and you'll need cluster admin rights to use. It supports the
HPC Web Portal. 

## Usage:

`didehpc` alone will give you a brief help message, including the 
available commands, currently: `bump`, `cancel`, `ldap`, `list`, `shunt`
and `waitfor`. Each of those commands will take other arguments, as below.

### bump - Raising job priority

Most of the time, if a job can be run, it will be run. Raising priority
only affects jobs where we have a particular condition, for example:-

* Our 32-core nodes are all part-busy, but we have some spare
  cores on one or more of them.
* Job A is queuing next, which wants an entire 32-core node and can't get
  one, so it waits. 
* Job B is queuing next, which is a single core job and wants to run for
  some reason on the 32-core node. The scheduler doesn't know which of 
  the 32-core nodes will free up first to give to Job A, and it won't
  risk being unfair to Job A which was submitted earlier, by allocating
  spare cores.

Raising the priority of Job B to above Job A will give the scheduler
permission to use the spare cores for Job B (at the potential expense
of Job A), which I will call `bumping`. 

In the syntaxes below, I'll use `<thing>` to specify something you
need, `|` to indicate alternative forms, and `[optional]` to indicate
something you can specify, but you don't have to. Hence:-

`didehpc bump <scheduler> <id | id1,id2,id3 | idx:idy> [priority]`

where `scheduler` is the name of the cluster headnode. `id` is a job id, 
and can be a single job, a comma-separated list of jobs, or a contiguous range
of jobs. `priority`, if given, is a number between `0` and `4000`, where `0`
is the lowest priority, `4000` is the highest. If omitted, then jobs will
have their priority raised by `10`.

### cancel - Mass cancellation of jobs

For simple command-line cancelling of a number of jobs - currently
for a named user. (We could consider wild-carding user here...)

`didehpc cancel <scheduler> <user> <id | id1,id2,id3 | idx:idy>`

This script outputs tab-separated data in the form 
`ID tab STATUS`

where `STATUS` is one of `OK` (successful cancel), `NOT_FOUND` where an id
was not found in the scheduler, `WRONG_USER` where the job belonged to 
someone other than <user>, or `WRONG_STATE` where the job was not running.

### ldap - Lookup group memberships

Used mainly internally, but this looks up which groups a user belongs to
in the DIDE domain controllers. This is used for determining which templates
a user has permission to submit to. 

`didehpc ldap <base64_user> <base64_password>`

Where the user and password and base-64 encoded in the standard way. The output
is a line-by-line list of domain groups the user belongs to, or `CREDENTIAL_ERROR`
if authentication failed.

### list - List current jobs

This gives us a table of running jobs, including their Id, Name, State, Resources
the jobs are using, the user (owner technically), starting time, submission time,
ending time, and the template used. We can limit jobs to a give number, or user.
For some reason, we have to specify a state.

`didehpc list <scheduler> <state> <user | *> <no_jobs | -1>`

where `<scheduler>` is the headnode, `<state>` is one of `Running`, `Queued`, `Finished`,
`Canceled`, `Failed`, (or less usefully `Canceling`, `Configuring` and `Finishing`). The
`<user>` can be a username, or `*` for wildcard. The `<no_jobs>` limits the number of
rows, or a negative number will return 32,767.

### shunt - Moving from one template to another

You may want to shift jobs from the template they were initially submitted with
to a different one. For example:-

* Someone has submitted a job requestion 32 cores, to GeneralNodes (which has
  no such nodes)
* GeneralNodes has a very long queue, but the 32Core template is entirely
  empty, such that a blast of jobs on the 32Core template wouldn't delay
  anyone else.

`didehpc shunt <scheduler> <id | id1,id2,id3 | idx:idy> <template>`

### waitfor

This is a blocking call, which sends you an email when a job (or bunch of jobs) finishes, 
and then terminates. So this needs to be called in another process if you want code to carry
on doing things while you are waiting.

`didehpc waitfor <scheduler> <base64_email> <base64_group_name> <id | id1,id2,id3 | idx:idy >`

where `<scheduler>` is the headnode, `<base64_email>` is who you'd like to inform, and
`<base64_group_name>` is some free text relating to the bunch of jobs you are intersted in. 
(Both of the previous are base64-encoded obviously). Then as usual, the ids for the jobs
come at the end, and when all of the mentioned ids finishes, an email will get sent.
