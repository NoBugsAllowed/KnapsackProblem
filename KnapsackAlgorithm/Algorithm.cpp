#include "Algorithm.h"

Algorithm::Algorithm(Knapsack &k, Element* r, int n)
{
    elements = r;
    knapsack = k;
    elementsCount = n;
    used = new bool[n];
    bestElements = new Element[n];
    for (int i = 0; i < n; i++)
    {
        used[i] = false;
        bestElements[i] = r[i];
    }
}

Algorithm::~Algorithm()
{
    delete used;
    delete bestElements;
}

bool Algorithm::compute(int number)
{
    callsCount++;
    actValue = knapsack.actValue;
    if (bestValue < actValue)
    {
        bestValue = actValue;
        for (int i = 0; i < elementsCount; i++)
        {
            if (used[i])
            {
                bestElements[i].used = true;
                bestElements[i].diverted = elements[i].diverted;
                bestElements[i].x = elements[i].x;
                bestElements[i].y = elements[i].y;
            }
        }
    }
    if (elementsOnKnapsack == elementsCount)
    {
        return true;
    }
    if (number >= elementsCount)
    {
        return false;
    }

    //TODO: zwrócić uwagę na przypadki gdy dane wielkści się nieparzyste
    int widthMaxCounter  = elementsOnKnapsack == 0 ? knapsack.width / 2 - elements[number].width / 2 + 1 : knapsack.width  - elements[number].width;
    int heightMaxCounter = elementsOnKnapsack == 0 ? knapsack.height / 2 - elements[number].height / 2 + 1: knapsack.height - elements[number].height;

    //przypadek gdy uwzględniamy dany klocek
    for (int i = 0; i <= widthMaxCounter; i++)
    {
        for (int j = 0; j <= heightMaxCounter; j++)
        {
            if (knapsack.putElement(&elements[number], i, j, false))
            {
                used[number] = true;
                elementsOnKnapsack++;

                if(compute(number+1))
                {
                    return true;
                }

                used[number] = false;
                elementsOnKnapsack--;
                knapsack.takeElement(&elements[number]);
            }
        }
    }
	//jeśli element nie jest kwadratem to połóż obrócony o 90 stopni
	if (elements[number].height != elements[number].width)
	{
		widthMaxCounter = elementsOnKnapsack == 0 ? knapsack.width / 2 - elements[number].height / 2 + 1 : knapsack.width - elements[number].height;
		heightMaxCounter = elementsOnKnapsack == 0 ? knapsack.height / 2 - elements[number].width / 2 + 1 : knapsack.height - elements[number].width;

		for (int i = 0; i <= widthMaxCounter; i++)
		{
			for (int j = 0; j <= heightMaxCounter; j++)
			{
				if (knapsack.putElement(&elements[number], i, j, true))
				{
					used[number] = true;
					elementsOnKnapsack++;

					if (compute(number + 1))
					{
						return true;
					}

					used[number] = false;
					elementsOnKnapsack--;
					knapsack.takeElement(&elements[number]);
				}
			}
		}
	}
    //przypadek gdy nie uwzględniamy danego klocka w układzie
    if (compute(number + 1))
    {
        return true;
    }
    return false;
}
