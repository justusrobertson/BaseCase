;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; RIPLEY World
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain VBASE3)
	(:requirements :strips)
	(:predicates 
		(character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (object ?x)
	    (location ?x)
		(color ?x ?y)
	)
	
	(:action move-location
	    :parameters (?mover ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (at ?mover ?oldlocation) (not (at ?mover ?location)) (not (tied ?mover))
				(location ?location) (connected ?location ?oldlocation) (nodoor ?location ?oldlocation)
				(location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
			)
	)
	
	(:action enter-location
	    :parameters (?mover ?entrance ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (at ?mover ?oldlocation) (not (at ?mover ?location)) (not (tied ?mover))
				(entrance ?entrance) (at ?entrance ?oldlocation) (to ?entrance ?location)
				(location ?location)
				(location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
			)
	)
	
	(:action move-location-door
	    :parameters (?mover ?door ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (at ?mover ?oldlocation) (not (at ?mover ?location)) (not (tied ?mover))
				(door ?door) (between ?door ?location ?oldlocation) (not (locked ?door))
				(location ?location) (connected ?location ?oldlocation)
				(location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
			)
	)
	
	(:action open-door
	    :parameters (?opener ?key ?door)
	    :precondition 
			(and 
				(character ?opener) (has ?opener ?key) (not (tied ?opener))
				(door ?door) (locked ?door)
				(key ?key) (opens ?key ?door)
				(not (type ?opener soldier))
			)
	    :effect
			(and 
				(not (locked ?door))
			)
	)
	
	(:action untie-person
	    :parameters (?untier ?untied ?location)
	    :precondition 
			(and 
				(character ?untier) (at ?untier ?location) (not (tied ?untier)) (not (captor ?untier ?untied))
				(character ?untied) (tied ?untied) (at ?untied ?location)
				(location ?location)
			)
	    :effect
			(and 
				(not (tied ?untied))
			)
	)
	
	(:action take-thing
	    :parameters (?taker ?thing ?location)
	    :precondition 
			(and 
				(character ?taker) (at ?taker ?location)
				(thing ?thing) (at ?thing ?location) (not (enchanted ?thing))
				(location ?location)
			)
	    :effect
			(and 
				(not (at ?thing ?location))
				(has ?taker ?thing)
			)
	)
)
