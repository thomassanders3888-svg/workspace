


```
c
sharp


public
 class
 Quest
 {

   
 public
 string
 title
;

   
 public
 string
 description
;

   
 public
 string
[]
 requirements
;

   
 public
 string
[]
 rewards
;

   
 public
 int
 gold
Reward
;

   
 public
 bool
 is
Completed
;

   
 public
 string
[]
 objectives
;


   
 public
 Quest
(string
 title
,
 string
 description
,
 string
[]
 requirements
,
 string
[]
 rewards
,
 int
 gold
Reward
,
 string
[]
 objectives
)
 {

       
 this
.title
 =
 title
;

       
 this
.description
 =
 description
;

       
 this
.require
ments
 =
 requirements
;

       
 this
.re
wards
 =
 rewards
;

       
 this
.gold
Reward
 =
 gold
Reward
;

       
 this
.is
Completed
 =
 false
;

       
 this
.object
ives
 =
 objectives
;

   
 }


   
 public
 void
 Complete
Quest
()
 {

       
 is
Completed
 =
 true
;

       
 Debug
.Log
("
Quest
 completed
:
 "
 +
 title
);

   
 }

}

``
`


This
 C
#
 class
 `
Quest
`
 represents
 a
 quest
 in
 a
 game
.
 It
 includes
 properties
 for
 the
 quest
's
 title
,
 description
,
 requirements
,
 rewards
,
 completion
 status
,
 and
 objectives
.
 The
 constructor
 initializes
 these
 properties
.
 There
's
 also
 a
 method
 `
Complete
Quest
()`
 that
 sets
 the
 `
is
Completed
`
 property
 to
 true
 and
 logs
 a
 message
 indicating
 the
 quest
 has
 been
 completed
.


You
 can
 use
 this
 class
 in
 your
 game
 by
 creating
 instances
 of
 it
 with
 appropriate
 parameters
,
 managing
 quest
 completion
,
 and
 possibly
 displaying
 quest
 details
 to
 players
.

