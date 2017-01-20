;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; UNIT-TEST world
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain UNIT-TEST)
	(:requirements :strips)
	(:predicates (character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (location ?x)
	)
	
	(:action move
	    :parameters (?mover ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (location ?location) (location ?oldlocation)
				(at ?mover ?oldlocation) (not (at ?mover ?location)) (alive ?mover)
				(connected ?location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
				(forall (?x)
					(when 
						(and
							(alive ?x)
							(at ?x ?oldlocation)
						)
						(and
							(fabulous ?x)
						)
					)
				)
			)
	)

	(:action take
	    :parameters (?taker ?thing ?place)
	    :precondition 
			(and 
				(character ?taker) (alive ?taker) (at ?taker ?place) (at ?thing ?place)
			)
	    :effect
			(and 
				(not (at ?thing ?place)) (has ?taker ?thing)
			)
	)
)
