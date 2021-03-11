import json
import os

import numpy as np
import pandas as pd
import requests

# Get the predict result of Azure model


def predict(data_csv, scoring_uri, key):
    data_list = data_csv.values.tolist()
    data = {'data': data_list}

    # Convert to JSON string
    input_data = json.dumps(data)

    # Set the content type
    headers = {'Content-Type': 'application/json'}
    # If authentication is enabled, set the authorization header
    headers['Authorization'] = f'Bearer {key}'

    # Make the request
    response = requests.post(scoring_uri, input_data, headers=headers)

    if response.status_code == 200:
        result = response.text
    else:
        print(response.json())
        print("Error code: %d" % (response.status_code))
        print("Message: %s" % (response.json()['message']))
        os.exit()

    return(result)


if __name__ == '__main__':
    # URL for the web service
    scoring_uri = 'http://32f74919-50e0-46cb-9c71-xxxxxxxxx.southeastasia.azurecontainer.io/score'
    # If the service is authenticated, set the key or token
    key = 'xxxxxxxxxxxxxxxxxxxxxxxxxxx'

    # Load the data from the csv file
    file_input = './data/data.csv'
    data_csv = pd.read_csv(file_input)

    # Get the predict result
    result = predict(data_csv, scoring_uri, key)

    # Save the result into csv file
    file_output = './data/output.csv'
    col_name = ['predict']
    output = pd.DataFrame(columns=col_name, data=eval(result))
    output.to_csv(file_output, index=False)
