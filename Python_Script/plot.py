import matplotlib.pyplot as plt
import numpy as np
import os
from pathlib import Path

script_dir = os.path.dirname(os.path.abspath(__file__))

CROP = False

INPUT = "run_2"
out_dir = "output/" + INPUT + "/"
out_dir = os.path.join(script_dir, out_dir)

# Define fixed colors for each group
COLORS = {"herbivore": "#EE9E64", "carnivore": "#71AFE2"}

ATTRIBUTES = ["Number", "Energy", "Speed", "Vision", "ReproduceProb"]

end = -1

# create directory
if not os.path.exists(out_dir):
    output_directory = Path(out_dir)
    output_directory.mkdir(parents=True, exist_ok=True)

# Loop through the files in pairs
for i, attribute in enumerate(ATTRIBUTES):
    animal_file = os.path.join(script_dir, "input/" + INPUT + "/Animal" + attribute + ".txt")
    predator_file = os.path.join(script_dir, "input/" + INPUT + "/Predator" + attribute + ".txt")

    # Load data from the two files in the pair
    data1 = np.loadtxt(animal_file)
    data2 = np.loadtxt(predator_file)

    if CROP:
        if attribute == "Number":
            indices = np.where(data1 < 50)
            if indices[0].size > 0:
                first_indice_1 = indices[0][0]
            else:
                first_indice_1 = len(data1)

            indices = np.where(data2 < 30)
            if indices[0].size > 0:
                first_indice_2 = indices[0][0]
            else:
                first_indice_2 = len(data2)
            
            end = min(first_indice_1, first_indice_2)
            
        data1 = data1[:end+1]
        data2 = data2[:end+1]

    x1 = np.arange(len(data1))
    x2 = np.arange(len(data2))

    # Plot the data from the two files in the pair with a fixed color and a line
    plt.plot(x1, data1, label='herbivore', color=COLORS['herbivore'])
    plt.plot(x2, data2, label='carnivore', color=COLORS['carnivore'])

    min_data = np.min([np.min(data1), np.min(data2)])
    max_data = np.max([np.max(data1), np.max(data2)])

    y_ticks = np.arange(min_data * 0.9, max_data * 1.1, (max_data - min_data) / 5)
    plt.yticks(y_ticks)

    # Add legend
    plt.legend()
    plt.grid(True)

    # Add labels to the axes
    plt.xlabel('Time')
    # plt.ylabel('')

    plt.title(attribute)

    file_name = out_dir + attribute + ".png"
    plt.savefig(file_name)

    # Show the plot
    plt.show()
