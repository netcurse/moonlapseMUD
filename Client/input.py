def compare_input(user_input, key):
    if user_input.is_sequence:
        return user_input.code == key
    return user_input == key