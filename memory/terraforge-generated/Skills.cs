 float
 level



To
 develop
 a
 basic
 system
 in
 Unity
 for
 mining
,
 wood
cut
ting
,
 and
 crafting
 with
 skills
,
 you
 would
 typically
 define
 an
 `
enum
`
 to
 represent
 the
 different
 skills
.
 Then
,
 create
 a
 class
 or
 struct
 to
 encaps
ulate
 the
 skill
 information
,
 such
 as
 the
 type
 of
 skill
,
 experience
,
 and
 level
.


Here
's
 a
 simplified
 example
:


```
c
sharp


using
 System
;

using
 UnityEngine
;


//
 Enum
 representing
 different
 types
 of
 skills


public
 enum
 Skill
Type
 {

   
 Mining
,

   
 Wood
cut
ting
,

   
 Crafting


}


//
 Class
 to
 hold
 information
 about
 a
 skill


[
System
.Serializable
]

public
 class
 Skill
Info
 {

   
 public
 Skill
Type
 type
;

   
 public
 int
 experience
;
 //
 Can
 be
 used
 for
 leveling
 up
 or
 other
 purposes


   
 public
 float
 level
;


   
 public
 Skill
Info
(S
kill
Type
 _
type
,
 int
 _
experience
,
 float
 _
level
)
 {

       
 type
 =
 _
type
;

       
 experience
 =
 _
experience
;

       
 level
 =
 _
level
;

   
 }

}


//
 Example
 usage
 in
 a
 MonoBehaviour


public
 class
 Skills
Manager
 :
 MonoBehaviour
 {

   
 private
 Skill
Info
[]
 skills
;


   
 void
 Start
()
 {

       
 //
 Initialize
 skills
 array
 with
 default
 values
 or
 based
 on
 some
 logic


       
 skills
 =
 new
 Skill
Info
[
3
];

       
 skills
[
0
]
 =
 new
 Skill
Info
(S
kill
Type
.M
ining
,
 
1
0
0
,
 
1
.
2
f
);

       
 skills
[
1
]
 =
 new
 Skill
Info
(S
kill
Type
.W
ood
cut
ting
,
 
5
0
,
 
0
.
8
f
);

       
 skills
[
2
]
 =
 new
 Skill
Info
(S

