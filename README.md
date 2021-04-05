# OpenMysticSimpleTcp #
A simple library for helping create TCP connections. Written in C#

### Useage ###

#### Events ####

There are two types of events that will occur for the main thread:
- TCP update (E.G. Connection Established, Connection Lost, ... )
- New data received.

These events will be received in order of occurrence.

### Design Goals ###

#### Multithreading ####
The library uses blocking TCP.  
The library is designed to accomodate the design pattern of one main thread
that has an update loop that gets called at regular intervals. During
this update the thread will check whether it has received any new data over
TCP.  
  
- The TCP read will be handled on a dedicated thread storing received data
to be handled by the main thread in the next update.  
  
- The TCP write will be handled on a dedicated thread where the main thread
sends data to write which is handled concurrently.  
  
- The initial connections will also be on a dedicated thread.


#### Memory Management ####
C# is a garbage collected language and as such all garbage will be collected until an unspecified time 
at which point the garbage collector will clean up the memory.    
  
- The goal here will be to minimize GC by reusing memory.
- Introduce a maximum limit for memory stored on the Read thread. 


