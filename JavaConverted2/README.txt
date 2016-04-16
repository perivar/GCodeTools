http://stackoverflow.com/questions/1575061/ga-written-in-java

/* 1. Init population */
POP_SIZE = number of individuals in the population
pop = newPop = []
for i=1 to POP_SIZE {
    pop.add( getRandomIndividual() )
}

/* 2. evaluate current population */
totalFitness = 0
for i=1 to POP_SIZE {
    fitness = pop[i].evaluate()
    totalFitness += fitness
}

while not end_condition (best fitness, #iterations, no improvement...)
{
    // build new population
    // optional: Elitism: copy best K from current pop to newPop
    while newPop.size()<POP_SIZE
    {
        /* 3. roulette wheel selection */
        // select 1st individual
        rnd = getRandomDouble([0,1]) * totalFitness
        for(idx=0; idx<POP_SIZE && rnd>0; idx++) {
            rnd -= pop[idx].fitness
        }
        indiv1 = pop[idx-1]
        // select 2nd individual
        rnd = getRandomDouble([0,1]) * totalFitness
        for(idx=0; idx<POP_SIZE && rnd>0; idx++) {
            rnd -= pop[idx].fitness
        }
        indiv2 = pop[idx-1]

        /* 4. crossover */
        indiv1, indiv2 = crossover(indiv1, indiv2)

        /* 5. mutation */
        indiv1.mutate()
        indiv2.mutate()

        // add to new population
        newPop.add(indiv1)
        newPop.add(indiv2)
    }
    pop = newPop
    newPop = []

    /* re-evaluate current population */
    totalFitness = 0
    for i=1 to POP_SIZE {
        fitness = pop[i].evaluate()
        totalFitness += fitness
    }
}

// return best genome
bestIndividual = pop.bestIndiv()     // max/min fitness indiv