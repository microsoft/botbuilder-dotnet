using System.Collections.Generic;

namespace Microsoft.Bot.Builder.GrammarChecker.Tests
{
    public class MockData
    {
        public static Dictionary<string, string> SyntaxDict = new Dictionary<string, string>()
        {
            {
                "She wants one apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                  2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                  3	one	_	NUM	CD	_	4	num	_	_
                  4	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants 1 apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                  2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                  3	1	_	NUM	CD	_	4	num	_	_
                  4	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants two apple",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                  2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                  3	two	_	NUM	CD	_	4	num	_	_
                  4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "She wants 2 apple",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                  2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                  3	2	_	NUM	CD	_	4	num	_	_
                  4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "It's about 12 mile away",
                @"1	It's	_	NOUN	NNS	_	0	ROOT	_	_
                  2	about	_	ADV	RB	_	3	quantmod	_	_
                  3	12	_	NUM	CD	_	4	num	_	_
                  4	mile	_	NOUN	NN	_	5	npadvmod	_	_
                  5	away	_	ADV	RB	_	1	advmod	_	_"
            },
            {
                "there is 54 cheap restaurants that allows childs",
                @"1	there	_	DET	EX	_	2	expl	_	_
                  2	is	_	VERB	VBZ	_	0	ROOT	_	_
                  3	54	_	NUM	CD	_	5	num	_	_
                  4	cheap	_	ADJ	JJ	_	5	amod	_	_
                  5	restaurants	_	NOUN	NNS	_	2	nsubj	_	_
                  6	that	_	DET	WDT	_	7	nsubj	_	_
                  7	allows	_	VERB	VBZ	_	5	rcmod	_	_
                  8	childs	_	NOUN	NNS	_	7	dobj	_	_"
            },
            {
                "I found a few places matching Hilton near you that have 5 star and have 2 or more star",
                @"1	I	_	PRON	PRP	_	2	nsubj	_	_
                2	found	_	VERB	VBD	_	0	ROOT	_	_
                3	a	_	DET	DT	_	5	det	_	_
                4	few	_	ADJ	JJ	_	5	amod	_	_
                5	places	_	NOUN	NNS	_	2	dobj	_	_
                6	matching	_	VERB	VBG	_	5	partmod	_	_
                7	Hilton	_	NOUN	NNP	_	6	dobj	_	_
                8	near	_	ADP	IN	_	2	prep	_	_
                9	you	_	PRON	PRP	_	8	pobj	_	_
                10	that	_	DET	WDT	_	11	nsubj	_	_
                11	have	_	VERB	VBP	_	2	dep	_	_
                12	5	_	NUM	CD	_	13	num	_	_
                13	star	_	NOUN	NN	_	11	dobj	_	_
                14	and	_	CONJ	CC	_	11	cc	_	_
                15	have	_	VERB	VBP	_	11	conj	_	_
                16	2	_	NUM	CD	_	19	num	_	_
                17	or	_	CONJ	CC	_	16	cc	_	_
                18	more	_	ADJ	JJR	_	16	conj	_	_
                19	star	_	NOUN	NN	_	15	dobj	_	_"
            },
            {
                "I found a few places matching  Hilton near you that have 5 star and have 2 or more star",
                @"1	I	_	PRON	PRP	_	2	nsubj	_	_
                2	found	_	VERB	VBD	_	0	ROOT	_	_
                3	a	_	DET	DT	_	5	det	_	_
                4	few	_	ADJ	JJ	_	5	amod	_	_
                5	places	_	NOUN	NNS	_	2	dobj	_	_
                6	matching	_	VERB	VBG	_	5	partmod	_	_
                7	Hilton	_	NOUN	NNP	_	6	dobj	_	_
                8	near	_	ADP	IN	_	2	prep	_	_
                9	you	_	PRON	PRP	_	8	pobj	_	_
                10	that	_	DET	WDT	_	11	nsubj	_	_
                11	have	_	VERB	VBP	_	2	dep	_	_
                12	5	_	NUM	CD	_	13	num	_	_
                13	star	_	NOUN	NN	_	11	dobj	_	_
                14	and	_	CONJ	CC	_	11	cc	_	_
                15	have	_	VERB	VBP	_	11	conj	_	_
                16	2	_	NUM	CD	_	19	num	_	_
                17	or	_	CONJ	CC	_	16	cc	_	_
                18	more	_	ADJ	JJR	_	16	conj	_	_
                19	star	_	NOUN	NN	_	15	dobj	_	_"
            },
            {
                "The apples is delicious",
                @"1	The	_	DET	DT	_	2	det	_	_
                2	apples	_	NOUN	NNS	_	4	nsubj	_	_
                3	is	_	VERB	VBZ	_	4	cop	_	_
                4	delicious	_	ADJ	JJ	_	0	ROOT	_	_"
            },
            {
                "The apples looks like delicious",
                @"1	The	_	DET	DT	_	2	det	_	_
                2	apples	_	NOUN	NNS	_	3	nsubj	_	_
                3	looks	_	VERB	VBZ	_	0	ROOT	_	_
                4	like	_	ADP	IN	_	3	prep	_	_
                5	delicious	_	ADJ	JJ	_	4	pobj	_	_"
            },
            {
                "125 of them is within one mile",
                @"1	125	_	NUM	CD	_	4	nsubj	_	_
                2	of	_	ADP	IN	_	1	prep	_	_
                3	them	_	PRON	PRP	_	2	pobj	_	_
                4	is	_	VERB	VBZ	_	0	ROOT	_	_
                5	within	_	ADP	IN	_	4	prep	_	_
                6	one	_	NUM	CD	_	7	num	_	_
                7	mile	_	NOUN	NN	_	5	pobj	_	_"
            },
            {
                "The apple are delicious",
                @"1	The	_	DET	DT	_	2	det	_	_
                2	apple	_	NOUN	NN	_	4	nsubj	_	_
                3	are	_	VERB	VBP	_	4	cop	_	_
                4	delicious	_	ADJ	JJ	_	0	ROOT	_	_"
            },
            {
                "The apple look like delicious",
                @"1	The	_	DET	DT	_	2	det	_	_
                2	apple	_	NOUN	NN	_	3	nsubj	_	_
                3	look	_	VERB	VBP	_	0	ROOT	_	_
                4	like	_	ADP	IN	_	3	prep	_	_
                5	delicious	_	ADJ	JJ	_	4	pobj	_	_"
            },
            {
                "She want two apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	want	_	VERB	VBP	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "He want two apple",
                @"1	He	_	PRON	PRP	_	2	nsubj	_	_
                2	want	_	VERB	VBP	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "It want two apple",
                @"1	It	_	PRON	PRP	_	2	nsubj	_	_
                2	want	_	VERB	VBP	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "Any of them want two apple",
                @"1	Any	_	DET	DT	_	4	nsubj	_	_
                2	of	_	ADP	IN	_	1	prep	_	_
                3	them	_	PRON	PRP	_	2	pobj	_	_
                4	want	_	VERB	VBP	_	0	ROOT	_	_
                5	two	_	NUM	CD	_	6	num	_	_
                6	apple	_	NOUN	NN	_	4	dobj	_	_"
            },
            {
                "Each of them want two apple",
                @"1	Each	_	DET	DT	_	4	nsubj	_	_
                2	of	_	ADP	IN	_	1	prep	_	_
                3	them	_	PRON	PRP	_	2	pobj	_	_
                4	want	_	VERB	VBP	_	0	ROOT	_	_
                5	two	_	NUM	CD	_	6	num	_	_
                6	apple	_	NOUN	NN	_	4	dobj	_	_"
            },
            {
                "Everyone want two apple",
                @"1	Everyone	_	NOUN	NN	_	2	nsubj	_	_
                2	want	_	VERB	VBP	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "Every one want two apple",
                @"1	Every	_	DET	DT	_	2	det	_	_
                2	one	_	NUM	CD	_	3	nsubj	_	_
                3	want	_	VERB	VBP	_	0	ROOT	_	_
                4	two	_	NUM	CD	_	5	num	_	_
                5	apple	_	NOUN	NN	_	3	dobj	_	_"
            },
            {
                "Each one want two apple",
                @"1	Each	_	DET	DT	_	2	det	_	_
                2	one	_	NUM	CD	_	3	nsubj	_	_
                3	want	_	VERB	VBP	_	0	ROOT	_	_
                4	two	_	NUM	CD	_	5	num	_	_
                5	apple	_	NOUN	NN	_	3	dobj	_	_"
            },
            {
                "Any one want two apple",
                @"1	Any	_	DET	DT	_	2	det	_	_
                2	one	_	NUM	CD	_	3	nsubj	_	_
                3	want	_	VERB	VBP	_	0	ROOT	_	_
                4	two	_	NUM	CD	_	5	num	_	_
                5	apple	_	NOUN	NN	_	3	dobj	_	_"
            },
            {
                "I wants two apple",
                @"1	I	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "We wants two apples",
                @"1	We	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "They wants two apples",
                @"1	They	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "You wants two apples",
                @"1	You	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	two	_	NUM	CD	_	4	num	_	_
                4	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "Many of them wants two apples",
                @"1	Many	_	ADJ	JJ	_	4	nsubj	_	_
                2	of	_	ADP	IN	_	1	prep	_	_
                3	them	_	PRON	PRP	_	2	pobj	_	_
                4	wants	_	VERB	VBZ	_	0	ROOT	_	_
                5	two	_	NUM	CD	_	6	num	_	_
                6	apples	_	NOUN	NNS	_	4	dobj	_	_"
            },
            {
                "She wants a apple",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	a	_	DET	DT	_	4	det	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "She wants a orange",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	a	_	DET	DT	_	4	det	_	_
                4	orange	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "She goes into an universality",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	goes	_	VERB	VBZ	_	0	ROOT	_	_
                3	into	_	ADP	IN	_	2	prep	_	_
                4	an	_	DET	DT	_	5	det	_	_
                5	universality	_	NOUN	NN	_	3	pobj	_	_"
            },
            {
                "an useful tool",
                @"1	an	_	DET	DT	_	3	det	_	_
                2	useful	_	ADJ	JJ	_	3	amod	_	_
                3	tool	_	NOUN	NN	_	0	ROOT	_	_"
            },
            {
                "She wants one hundred apple",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	one	_	NUM	CD	_	4	number	_	_
                4	hundred	_	NUM	CD	_	5	num	_	_
                5	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "She wants twenty-one apple",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	twenty-one	_	ADJ	JJ	_	4	amod	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "She wants the fifth apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	fifth	_	ADJ	JJ	_	5	amod	_	_
                5	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants the twenty-first apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	twenty-first	_	ADJ	JJ	_	5	amod	_	_
                5	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants the 1st apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	1st	_	ADJ	JJ	_	5	amod	_	_
                5	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants the 2nd apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	2nd	_	ADJ	JJ	_	5	amod	_	_
                5	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants the 3rd apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	3rd	_	ADJ	JJ	_	5	amod	_	_
                5	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "She wants the 4th apples",
                @"1	She	_	PRON	PRP	_	2	nsubj	_	_
                2	wants	_	VERB	VBZ	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	4th	_	ADJ	JJ	_	5	amod	_	_
                5	apples	_	NOUN	NNS	_	2	dobj	_	_"
            },
            {
                "Mary want the apple",
                @"1	Mary	_	NOUN	NNP	_	2	nsubj	_	_
                2	want	_	VERB	VBP	_	0	ROOT	_	_
                3	the	_	DET	DT	_	4	det	_	_
                4	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "Tom want the twenty-first apple",
                @"1	Tom	_	NOUN	NNP	_	2	nsubj	_	_
                2	want	_	VERB	VBP	_	0	ROOT	_	_
                3	the	_	DET	DT	_	5	det	_	_
                4	twenty-first	_	ADJ	JJ	_	5	amod	_	_
                5	apple	_	NOUN	NN	_	2	dobj	_	_"
            },
            {
                "but none have 3 or more stars",
                @"1	but	_	CONJ	CC	_	0	ROOT	_	_
                2	none	_	NOUN	NN	_	3	nsubj	_	_
                3	have	_	VERB	VBP	_	0	ROOT	_	_
                4	3	_	NUM	CD	_	7	num	_	_
                5	or	_	CONJ	CC	_	4	cc	_	_
                6	more	_	ADJ	JJR	_	4	conj	_	_
                7	stars	_	NOUN	NNS	_	3	dobj	_	_"
            },
            {
                "That is really cool",
                @"1	That	_	DET	DT	_	4	nsubj	_	_
                2	is	_	VERB	VBZ	_	4	cop	_	_
                3	really	_	ADV	RB	_	4	advmod	_	_
                4	cool	_	ADJ	JJ	_	0	ROOT	_	_"
            },
            {
                "Do you want to have a try",
                @"1	Do	_	VERB	VBP	_	3	aux	_	_
                2	you	_	PRON	PRP	_	3	nsubj	_	_
                3	want	_	VERB	VB	_	0	ROOT	_	_
                4	to	_	PRT	TO	_	5	aux	_	_
                5	have	_	VERB	VB	_	3	xcomp	_	_
                6	a	_	DET	DT	_	7	det	_	_
                7	try	_	NOUN	NN	_	5	dobj	_	_"
            },
            {
                "Do you wants to have a try",
                @"1	Do	_	VERB	VBP	_	3	aux	_	_
                2	you	_	PRON	PRP	_	3	nsubj	_	_
                3	want	_	VERB	VB	_	0	ROOT	_	_
                4	to	_	PRT	TO	_	5	aux	_	_
                5	have	_	VERB	VB	_	3	xcomp	_	_
                6	a	_	DET	DT	_	7	det	_	_
                7	try	_	NOUN	NN	_	5	dobj	_	_"
            }
        };
    }
}
