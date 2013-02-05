# Programmable Multicellular Organism Simulation
## Background
A dynamic system with relatively simple rules can lead to interesting and often
unexpected emergent behaviours. The most fascinating and diverse product that
has emerged from this universe is arguably life, with its ability to constantly
adapt and reinvent itself to exist in disparate and extreme environments.
The evolution of multicellularity opened the way to the development of far more
complex organisms that overcome complex problems in their environment.

This project will attempt to implement a simulation of simple abstracted cells
which are able to interact in a way that supports the emergence of symbiotic
multicellular structures. A visualisation of the simulation will also be
produced to provide an insight into the configurations of the emerging
organisms and their behaviours.

## Goals
### Simulation
A program will be produced to simulate a collection of simple cells existing on
a Cartesian plane. Each cell requires the absorption of nutrients from its
environment in order to function. Cells will contain a sequence of genetic code
which encodes a program that governs the activities of that cell. Possible
actions include self propulsion, attaching to and detaching from neighbouring
cells, transferring nutrients to attached cells, sending messages to attached
cells, and reproduction through cell division. These interactions should be
able to support a large range of possible emergent behaviours, including
predatory, symbiotic and parasitic relationships between organisms.

There will be the ability to introduce the possibility of mutation during
cellular division, which will lead to the emulation of natural selection. There
will also be the possibility of initiating a simulation with a collection of
pre-designed organisms; the genetic code for these organisms would be produced
through assembling programs written in a simple programming language. This
could be branched into an educational programming game where players attempt to
implement organisms to compete against those designed by opponents.

### Visualisation
Software designed to visually represent the organisms and their environment 
will be produced. The world will be displayed in two dimensions, with the
positions of individual cells visible. Cells may be selected to observe their
properties, such as current nutrient levels, their genetic code, and what part
of their code is currently being executed. The interface will be designed to be
as accessible as possible to allow for the usage of the application in
education, but also allow for a wide range of modifications to the simulated
environment.

## Implementation
The software will be implemented in the C# programming language. This project
will extend and improve on a previous body of work started in September 2012
and postponed shortly after due to a lack of available time and additional
research needed relating to simulating rigid body physical interactions. The
existing solution includes a full implementation of the virtual machine that
executes the genetic code of each cell, although the instruction set used will
almost certainly be altered and the simulation itself redesigned to support
multi-threading.
