#include <iostream>
#include <fstream>
#include <string>
#include <Windows.h>
#include <chrono>
#include "Algorithm.h"
#include "Knapsack.h"


using namespace std;

int main(int argc, char* argv[])
{
	//Sleep(15000);
	//Windows
	char pBuf[256];
	size_t len = sizeof(pBuf);
	int bytes = GetModuleFileName(NULL, pBuf, len);
	string inFilePath = "";
	int idx = -1;
	for (int i = len - 1; i >= 0; i--)
	{
		if (pBuf[i] == '\\')
		{
			idx = i;
			break;
		}
	}
	for (int i = 0; i <= idx; i++) 
	{
		inFilePath += pBuf[i];
	}
	inFilePath += argv[1];
	//Windows

	ifstream infile;
	infile.open(inFilePath);
	int kwidth, kheight, n;
	int w, h, val;

	try
	{
		infile >> kwidth >> kheight;
		infile >> n;
		Element* elements = new Element[n];
		for (int i = 0; i < n; i++)
		{
			infile >> w >> h >> val;
			elements[i].width = w;
			elements[i].height = h;
			elements[i].value = val;
			elements[i].diverted = false;
			elements[i].used = false;
			elements[i].onTable = false;
			elements[i].x = 0;
			elements[i].y = 0;
		}
		infile.close();

		Knapsack knapsack(kwidth, kheight);
		Algorithm algorithm(knapsack, elements, n);

		auto start_time = std::chrono::high_resolution_clock::now();

		algorithm.compute();

		auto end_time = std::chrono::high_resolution_clock::now();
		auto time = end_time - start_time;
		double ms = time / std::chrono::milliseconds(1);


		Knapsack k2(kwidth, kheight);
		for (int i = 0; i < n; i++)
		{
			if (algorithm.bestElements[i].used)
			{
				k2.putElement(&algorithm.bestElements[i], algorithm.bestElements[i].x, algorithm.bestElements[i].y, algorithm.bestElements[i].diverted);
			}
		}

		ofstream outfile;
		outfile.open(argv[2]);
		outfile << ms << std::endl;
		outfile << algorithm.bestValue << std::endl;
		for (int j = kheight - 1; j >= 0; j--)
		{
			outfile << k2.used[0][j];
			for (int i = 1; i < kwidth; i++)
			{
				outfile << " " << k2.used[i][j];
			}
			outfile << std::endl;
		}
		outfile.close();
		delete elements;
	}
	catch (...) { }
	return 0;
}
