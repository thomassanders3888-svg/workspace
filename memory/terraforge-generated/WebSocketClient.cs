


In
 Unity
,
 I
'm
 trying
 to
 create
 a
 WebSocket
 client
 using
 C
#.
 The
 goal
 is
 to
 have
 an
 asynchronous
 `
Connect
`,
 `
Send
`,
 and
 `
Receive
`
 method
.
 Additionally
,
 I
 want
 the
 ability
 to
 close
 the
 connection
 and
 reconnect
 using
 cor
outines
.
 Here
's
 my
 code
:


```
c
sharp


using
 System
;

using
 System
.Collections
;

using
 System
.Net
.Web
S
ockets
;

using
 System
.Text
;

using
 UnityEngine
;


public
 class
 WebSocket
Client
 :
 MonoBehaviour


{

   
 private
 Client
WebSocket
 web
Socket
;


   
 private
 void
 Start
()

   
 {

       
 StartCoroutine
(
Start
Connection
());

   
 }


   
 public
 IEnumerator
 Start
Connection
()

   
 {

       
 web
Socket
 =
 new
 Client
WebSocket
();


       
 try


       
 {

           
 var
 server
Uri
 =
 new
 Uri
("
ws
://
localhost
:
8
0
8
0
");

           
 yield
 return
 Connect
Async
(web
Socket
,
 server
Uri
);

           
 Debug
.Log
("
Connected
 to
 the
 WebSocket
 server
.");


           
 while
 (
web
Socket
.State
 ==
 WebSocket
State
.Open
)

           
 {

               
 var
 message
 =
 "
Hello
,
 Server
!";

               
 yield
 return
 Send
Async
(web
Socket
,
 message
);


               
 byte
[]
 buffer
 =
 new
 byte
[
1
0
2
4
 *
 
4
];

               
 Array
Segment
<byte
>
 segment
 =
 new
 Array
Segment
<byte
>(
buffer
);

               
 WebSocket
Receive
Result
 result
;

               
 yield
 return
 Receive
Async
(web
Socket
,
 segment
,
 out
 result
);

               
 Debug
.Log
("
Received
:
 "
 +
 Encoding
.UTF
8
.GetString
(buffer
,
 
0
,
 result
.Count
));

           
 }

       
 }

       
 catch
 (
Exception
 ex
)

       
 {

           
 Debug
.LogError
(ex
.Message
);

       
 }

       
 finally


       
 {

           
 web
Socket
.Close
Async

