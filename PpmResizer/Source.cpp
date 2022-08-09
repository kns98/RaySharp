/*Name: Yen Pham
Assignment 3
CS 5390
This program resizes a ppm image to 1/4 and 4 times of the original image

to compile: go to Putty using cse machine: cse01.cse.unt.edu
gcc pham3.c -lm

to run: ./a.out

*/

#include <ctype.h>
#include <errno.h>
#include <math.h> 
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>


typedef struct RGB
{
	unsigned char R, G, B;
}RGBpixel;

typedef struct Image
{
	int magic_identifier;
	int width, height;
	int max_value;
	RGBpixel* data;
}PPMimage;

void ReadPPM(char* filename, PPMimage* image);
void CreatePPM(PPMimage original_img, PPMimage* small_img);
void WritePPM(PPMimage small_img);


int main()
{
	char infile[50];
	//size_t len = 500;
	char temp_line[500];
	char* temp_input;
	PPMimage original_img;
	PPMimage small_img;
	PPMimage large_img;

	// Prompt user to enter ppm image
	do
	{
		printf("Enter ppm file name: ");
		scanf("%s", infile);
		if (strstr(infile, ".ppm") == NULL)
		{
			printf("Please input a .ppm file\n");
		}
	} while (strstr(infile, ".ppm") == NULL);

	// Call functions
	ReadPPM(infile, &original_img);
	CreatePPM(original_img, &small_img);
	WritePPM(small_img);
	return 0;
}

void ReadPPM(char* filename, PPMimage* image) //open and read ppm file
{
	FILE* fp = fopen(filename, "rb");

	if (fp == NULL)
	{
		printf("Error opening file. No file existed with that name\n");
		exit(0);
	}

	//read header
	char c = fgetc(fp);
	if (c != 'P')
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}
	c = fgetc(fp);
	if (c == '3') // get maic identifier
	{
		(*image).magic_identifier = 3;
	}
	else if (c == '6')
	{
		(*image).magic_identifier = 6;
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	c = fgetc(fp);
	//if the next character is comment or space, "eat" that character and continue
	if ((c == '#') || isspace(c))
	{
		while ((c == '#') || isspace(c))
		{
			if (c == '#')
			{
				while (c != '\n')
				{
					c = fgetc(fp);
				}
			}
			else
			{
				while (isspace(c))
				{
					c = fgetc(fp);
				}
			}
		}
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	//read image width
	int count = 1;
	char digits[17];
	if (isdigit(c))
	{
		digits[0] = c;
		while (isdigit(c) && (count < 16))
		{
			c = fgetc(fp);
			digits[count] = c;
			count++;
		}
		digits[count] = '\0';
		(*image).width = atoi(digits);
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	//check if there is any comment or space after the space. If yes, eat the space and/or comment
	if (isspace(c))
	{
		while ((c == '#') || isspace(c))
		{
			if (c == '#')
			{
				while (c != '\n')
				{
					c = fgetc(fp);
				}
			}
			else
			{
				while (isspace(c))
				{
					c = fgetc(fp);
				}
			}
		}
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	//read image height
	memset(digits, 0, 17);
	count = 1;
	if (isdigit(c))
	{
		digits[0] = c;
		while (isdigit(c) && (count < 16))
		{
			c = fgetc(fp);
			digits[count] = c;
			count++;
		}
		digits[count] = '\0';
		(*image).height = atoi(digits);
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	//check if there is any comment or space after the space. If yes, eat the space and/or comment
	if (isspace(c))
	{
		while ((c == '#') || isspace(c))
		{
			if (c == '#')
			{
				while (c != '\n')
				{
					c = fgetc(fp);
				}
			}
			else
			{
				while (isspace(c))
				{
					c = fgetc(fp);
				}
			}
		}
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	//read image max value
	memset(digits, 0, 17);
	count = 1;
	if (isdigit(c))
	{
		digits[0] = c;
		while (isdigit(c) && (count < 16))
		{
			c = fgetc(fp);
			digits[count] = c;
			count++;
		}
		digits[count] = '\0';
		(*image).max_value = atoi(digits);
	}
	else
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	//printf("%d %d %d %d\n", (*image).magic_identifier, (*image).width, (*image).height, (*image).max_value);

	//check if there is any comment or space after the space. If yes, eat the space and/or comment
	if (!isspace(c))
	{
		printf("%s is not a valid ppm file\n", filename);
		exit(0);
	}

	(*image).data = static_cast<RGBpixel*>(malloc(sizeof(RGBpixel) * (*image).width * (*image).height));
	int temp;
	count = 0;

	//read image data
	if ((*image).magic_identifier == 3)
	{
		for (int i = 0; i < (*image).width * (*image).height; i++)
		{
			fscanf(fp, "%d", &temp);
			(*image).data[i].R = temp;
			fscanf(fp, "%d", &temp);
			(*image).data[i].G = temp;
			fscanf(fp, "%d", &temp);
			(*image).data[i].B = temp;
		}
	}

	if ((*image).magic_identifier == 6)
	{
		fread((*image).data, sizeof(RGBpixel), (*image).width * (*image).height, fp);
	}

	/* test
	for (int i = 0; i < (*image).width * (*image).height; i++)
	{
		printf("%d %d %d\n", (*image).data[i].R, (*image).data[i].G, (*image).data[i].B);
	}
	*/
}

void CreatePPM(PPMimage original_img, PPMimage* small_img)
{
	//Create small image
	(*small_img).magic_identifier = original_img.magic_identifier;
	(*small_img).width = original_img.width / 2;
	(*small_img).height = original_img.height / 2;
	(*small_img).max_value = original_img.max_value;
	(*small_img).data = static_cast<RGBpixel*>(malloc(sizeof(RGBpixel) * original_img.width * original_img.height / 4));

	//initial all pixels to white
	for (int k = 0; k < (*small_img).width * (*small_img).height; k++)
	{
		(*small_img).data[k].R = 255;
		(*small_img).data[k].G = 255;
		(*small_img).data[k].B = 255;
	}

	int i, n;
	i = 0;
	for (int g = 0; g < (*small_img).width; g++)
	{
		n = 2 * g;
		for (int t = 0; t < (*small_img).height; t++)
		{
			i = t * (*small_img).width + g;

			//printf("i: %d n: %d \n", i, n); 
			(*small_img).data[i].R = round((double)(original_img.data[n].R + original_img.data[n + 1].R + original_img.data[n + original_img.width].R + original_img.data[1 + n + original_img.width].R) / 4.0);
			(*small_img).data[i].G = round((double)(original_img.data[n].G + original_img.data[n + 1].G + original_img.data[n + original_img.width].G + original_img.data[1 + n + original_img.width].G) / 4.0);
			(*small_img).data[i].B = round((double)(original_img.data[n].B + original_img.data[n + 1].B + original_img.data[n + original_img.width].B + original_img.data[1 + n + original_img.width].B) / 4.0);

			n += 2 * original_img.width;
		}
	}

	
}

void WritePPM(PPMimage small_img)
{
	int counter;
	FILE* fp = fopen("small.ppm", "wb");

	// Output a new small image
	if (small_img.magic_identifier == 3)
	{
		fprintf(fp, "P3\n");
		fprintf(fp, "%d %d %d\n", small_img.width, small_img.height, small_img.max_value);

		counter = 1;
		for (int i = 0; i < small_img.width * small_img.height; i++)
		{
			fprintf(fp, "%d %d %d ", small_img.data[i].R, small_img.data[i].G, small_img.data[i].B);

			if (counter == small_img.width)
			{
				fprintf(fp, "\n");
				counter = 1;
			}
			else
			{
				counter++;
			}
		}
	}
	else if (small_img.magic_identifier == 6)
	{
		fprintf(fp, "P6\n");
		fprintf(fp, "%d %d %d\n", small_img.width, small_img.height, small_img.max_value);
		fwrite(small_img.data, 3 * small_img.width, small_img.height, fp);
	}

}
