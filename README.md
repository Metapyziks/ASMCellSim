ASMCellSim
==========

Today, I shall be God.

Instruction Mnemonics
---------------------

	PUSH {val}              - Push literal
    POP                     - Discard top of stack
	PEEK                    - Push a copy of item at top of stack
	COPY offset             - Push item in stack offset from the top
	
	LLOD index		        - Push value from index in current block
	LSTO index val          - Store value to index in current block
	RLOD pindex index       - Push value from index in block at pindex
	RSTO pindex index val   - Store value to index in block at pindex
	
	JUMP index              - Set PC to index
	JPIF index val          - If val is not 0x00, set PC to index
	CALL pindex				- Push PC and PI, jump to block at pindex
	RTN                     - Restore PC and PI from stack
	
	SLP  ticks              - Idle for given number of ticks
	
	INC  a                  - Push a+1
	DEC  a                  - Push a-1
	
	AND  a b                - Push a&b
	OR   a b                - Push a|b
	XOR  a b                - Push a^b
	NOT  a                  - Push ~a
	
	ADD  a b                - Push a+b
	SUB  a b                - Push a-b
	MUL  a b                - Push a*b
	DIV  a b                - Push a/b
	
	LSFT a b                - Push a<<b
	LLOP a b                - Push a<<b|a>>(8-b)
	RSFT a b                - Push a>>b
	RLOP a b                - Push a>>b|a<<(8-b)
	
	EQUL a b                - Push 0x01 if a==b, else 0x00
	GRT  a b                - Push 0x01 if a>b, else 0x00
	LST  a b                - Push 0x01 if a<b, else 0x00
	
	ECHK                    - Push most signif. byte of energy stored
	EGIV hectant amount     - Give given amount of energy to hectant
	
	SCAN hectant            - Push trace result from scan at hectant
	JET  hectant power      - Fire a jet from hectant with given power
	
	LCHK hectant            - Push link state at hectant
	LINK hectant            - Attempt to link with cell at hectant
	
	MCHK hectant            - Push 0x01 if message waiting, else 0x00
	MGET hectant            - Push message from link at hectant
	MSND hectant message    - Send message to link at hectant
	
	DUP  hectant            - Create a new cell at hectant
