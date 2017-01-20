(define (problem 01)
	(:domain VBASE3)
	(:objects 
		RIPLEY FOX SOLDIER BLART PAUL
		MAIN_SCREEN
		CELL_DOOR
		ARMORY_DOOR
		EXIT_DOOR ENTRANCE_DOOR REAL_ESCAPE_DOOR
		CELL ENTRANCE HUB JAIL ARMORY DORM EXIT REALESCAPE
		CELL_DOOR_KEY
	)
	(:init    
		(player ripley) (character ripley) (at ripley entrance) (type ripley ripley) (inventory)
		(character fox) (at fox cell) (type fox prisoner) (tied fox)
		(character soldier) (at soldier hub) (type soldier soldier) (captor soldier fox)
		(character blart) (at blart cell) (type blart soldier) (captor blart fox)
		(character paul) (at paul armory) (type paul soldier) (captor paul fox)
		
		(computer main_screen) (at main_screen armory) (type main_screen computer)
		(key cell_door_key) (thing cell_door_key) (type cell_door_key keycard) 
			(opens cell_door_key cell_door)
			(opens cell_door_key exit_door) 
			(at cell_door_key entrance)

		
		(door cell_door) (thing cell_door) (type cell_door door) 
			(between cell_door jail cell) 
			(between cell_door cell jail) 
			(locked cell_door)
			
		(door armory_door) (thing armory_door) (type armory_door door) 
			(between armory_door jail armory)
			(between armory_door armory jail)
			
		(door exit_door) (thing exit_door) (type exit_door door) 
			(between exit_door hub exit)
			(between exit_door exit hub) 
			(locked exit_door)
			
		(door entrance_door) (thing entrance_door) (type entrance_door door) 
			(between entrance_door entrance hub)
			(between entrance_door hub entrance)
			
		(location entrance) (type entrance specialbase) 
			(connected entrance hub)
		
		(location cell) (type cell specialbase) 
			(connected cell jail)
			(connected cell realescape)
			
		(location jail) (type jail base) 
			(connected jail cell) 
			(connected jail hub) 
				(nodoor jail hub)
			(connected jail armory)
			
		(location hub) (type hub base) 
			(connected hub jail)
				(nodoor hub jail)
			(connected hub entrance) 
			(connected hub exit)
			(connected hub dorm)
				(nodoor hub dorm)
			
		(location exit) (type exit woods) 
			(connected exit hub)
			
		(location dorm) (type dorm base) 
			(connected dorm hub) 
			(nodoor dorm hub)
			
		(location armory) (type armory base) 
			(connected armory jail)
			
		(door real_escape_door) (thing real_escape_door) (type real_escape_door door) 
			(between real_escape_door cell realescape)
			(between real_escape_door realescape cell)
			(locked real_escape_door)
			
		(location realescape) (type realescape cave) 
			(connected realescape cell)
	)
	(:goal 
		(and
			(at soldier dorm) (at fox exit) (at ripley exit) (at blart entrance)
		)
	)
)