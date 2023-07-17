from typing import Callable
from input import compare_input
from blessed import Terminal

class Widget:
    """An interselected component in a view, e.g. a TextField."""

    def __init__(self, controller):
        self.controller = controller
        self.term: Terminal = controller.term
        self.selected = False
        self.width = self.term.width

    def activate(self):
        self.selected = True

    def draw(self):
        pass

    def handle_input(self, user_input):
        pass


class Button(Widget):
    def __init__(self, controller, text, action: Callable, *params):
        super().__init__(controller)
        self.text = text
        self.action = action
        self.params = params
        self.width = len(text)

    def activate(self):
        super().activate()
        self.action(*self.params)

    def draw(self):
        # If the button text is longer than the width, truncate it
        if len(self.text) > self.width:
            self.text = self.text[:self.width]
        
        # Draw the text centered in the button
        self.text = self.text.center(self.width)
        if self.selected:
            # Highlight the button text if it's selected
            print(self.term.on_yellow(self.text))
        else:
            print(self.text)


class CheckBox(Widget):
    def __init__(self, controller, text, checked=False):
        super().__init__(controller)
        self.text = text
        self.checked = checked
        self.width = len(text + ' [ ]')
    
    def draw(self):
        if self.checked:
            state = "[x]"
        else:
            state = "[ ]"

        # If the checkbox text is longer than the width, truncate it
        if len(self.text) > self.width:
            self.text = self.text[:self.width - len(state)]
        
        # Draw the text and checkbox state centered
        self.text = self.text.center(self.width - len(state))
        if self.selected:
            # Highlight the checkbox text if it's selected
            print(self.term.on_yellow(self.text + ' ' + state))
        else:
            print(self.text + ' ' + state)

    def activate(self):
        super().activate()
        self.checked = not self.checked

class TextBox(Widget):
    def __init__(self, controller, num_lines=10):
        super().__init__(controller)
        self.lines = []
        self.num_lines = num_lines
        self._top_line_idx = 0
        self.box_width = self.width - 2 # Subtract 2 for the box borders

    def draw(self):
        super().draw()
        print_row = lambda row: print(row)
        if self.selected:
            print_row = lambda row: print(self.term.yellow(row))

        print_row('┌' + '─' * self.width + '┐')
        
        for i in range(self._top_line_idx, min(self._top_line_idx + self.num_lines, len(self.lines))):
            line = self.lines[i]

            # If the line is shorter than width, pad it with spaces
            if len(line) < self.width:
                line += ' ' * (self.width - len(line))
            # If the line is longer than width, truncate it
            elif len(line) > self.width:
                line = line[:self.width]

            print_row('│' + line + '│')
        
        # Fill in empty lines if there are less than num_lines
        for _ in range(len(self.lines), self._top_line_idx + self.num_lines):
            print_row('│' + ' ' * self.width + '│')

        print_row('└' + '─' * self.width + '┘')

        # Show a prompt if there are more lines below
        if (self._top_line_idx < len(self.lines) - self.num_lines):
            diff = len(self.lines) - self.num_lines - self._top_line_idx
            prompt = f'({diff} more ↓)'
            if self.selected:
                prompt = self.term.on_yellow(prompt)
            print(prompt)
        else:
            print() # Reserve this space for the prompt

    def add_line(self, line):
        self.lines.append(line)

    def set_top_line_idx(self, idx):
        self._top_line_idx = idx

    def scroll_up(self):
        self._top_line_idx = max(0, self._top_line_idx - 1)
    
    def scroll_down(self):
        self._top_line_idx = min(len(self.lines) - self.num_lines, self._top_line_idx + 1)

    def clear(self):
        self.lines = []

    def handle_input(self, user_input):
        super().handle_input(user_input)
        if compare_input(user_input, self.term.KEY_SUP):
            self.scroll_up()
        elif compare_input(user_input, self.term.KEY_SDOWN):
            self.scroll_down()


class TextField(Widget):
    def __init__(self, controller, label, censored=False, on_enter_action: Callable = lambda: None, *on_enter_action_params):
        super().__init__(controller)
        self.label = label
        self.text = ''
        self.censored = censored
        self._on_enter_action = on_enter_action
        self._on_enter_action_params = on_enter_action_params

    def draw(self):
        super().draw()
        label = self.label
        if self.censored:
            content = '*' * len(self.text)
        else:
            content = self.text
        
        if self.selected:
            label = self.term.on_yellow(self.label)
            content += self.term.blink('_')

        # Print label and content together
        print(f'{label} {content}')

    def clear(self):
        self.text = ''

    def handle_input(self, user_input):
        super().handle_input(user_input)
        if compare_input(user_input, self.term.KEY_ENTER):
            self._on_enter_action(*self._on_enter_action_params)
        elif compare_input(user_input, self.term.KEY_BACKSPACE):
            self.text = self.text[:-1]
        elif compare_input(user_input, self.term.KEY_DELETE):
            self.text = self.text[1:]
        elif not user_input.is_sequence:
            self.text += user_input


class HorizontalContainer(Widget):
    def __init__(self, controller, *weights):
        super().__init__(controller)
        self.children = [None] * len(weights)
        self.weights = weights
        self.validate_weights(weights)

        self.cursor_idx = 0
        self.n_children = 0

    def validate_weights(self, weights):
        from math import gcd, ceil
        from functools import reduce

        def lcm(*numbers):
            """Return lowest common multiple."""    
            def lcm(a, b):
                """Return lowest common multiple of two numbers."""    
                return (a * b) // gcd(a, b)

            return reduce(lcm, numbers, 1)

        def redistribute_weights(*weights):
            """Redistribute weights to make their inverses sum to 1 by adding integer weight(s) to fill the remaining space."""
            # E.g.1. if weights == (5, 5, 4), then the lcm is 20.
            # E.g.2. if weights == (3,), then the lcm is 3. 
            lcm_of_weights = lcm(*weights)
            
            # E.g.1. in scenario above, weights_in_proportion_to_lcm == (4, 4, 5) since 1/5 = 4/20 and 1/4 = 5/20.
            # E.g.2. in scenario above, weights_in_proportion_to_lcm == (1,) since 1/1 = 1/3.
            weights_in_proportion_to_lcm = tuple(lcm_of_weights / weight for weight in weights)

            # E.g.1. in scenario above, missing_weight_in_proportion_to_lcm == 7 since 20 - (4 + 4 + 5) == 7.
            # E.g.2. in scenario above, missing_weight_in_proportion_to_lcm == 2 since 3 - (1) == 2.
            missing_weight_in_proportion_to_lcm = lcm_of_weights - sum(weights_in_proportion_to_lcm)
            if missing_weight_in_proportion_to_lcm == 0:
                return

            # E.g.1. in scenario above, 7/20 cannot be reduced to a fraction with a 1 in the numerator
            # since 20 is not divisible by 7, so we need to express it as the sum of fractions with 
            # 1 in the numerator, i.e. 5/20 + 2/20 = 1/4 + 1/10.
            # E.g.2. in scenario above, 2/3 cannot be reduced to a fraction with a 1 in the numerator
            # since 3 is not divisible by 2, so we need to express it as the sum of fractions with
            # 1 in the numerator, i.e. 1/3 + 1/3.
            self.weights = weights + break_into_egyption_fractions(missing_weight_in_proportion_to_lcm, lcm_of_weights)

        def break_into_egyption_fractions(numerator, denominator):
            """Takes a fraction e.g. (7,20) and returns a tuple of fractions with 1 in the numerator that sum to the original fraction, 
            E.g.1. (7,20) returns ((1,4), (1,10)) (since 7/20 == 5/20 + 2/20 = 1/4 + 1/10)))
            E.g.2. (2, 3) returns ((1, 3), (1, 3)) (since 2/3 == 1/3 + 1/3))
            An Egyptian fraction is a finite sum of distinct fractions where the numerator is 1 and the denominator is a positive integer, and two different fractions have different denominators.

            The method to convert a fraction into an Egyptian fraction is described as follows:

            Begin with the fraction to be converted.
            Find the greatest possible unit fraction that can be subtracted from it (i.e., the reciprocal of the ceiling of the denominator divided by the numerator), and subtract this from the fraction to get a new fraction.
            Repeat the process with the new fraction.
            In mathematical terms, given a fraction a/b where a < b, the largest unit fraction that can be subtracted from a/b is 1/⌈b/a⌉. After subtracting, the new fraction is (a*⌈b/a⌉ - b) / (b*⌈b/a⌉).
            """
            fractions = []  # Store fractions

            while numerator != 0:
                x = ceil(denominator / numerator)
                fractions.append(x)
                numerator = x * numerator - denominator
                denominator = denominator * x

            return tuple(fractions)
        
        total_inverse_weight = sum(1/weight for weight in weights)
        if total_inverse_weight > 1:
            raise ValueError("Sum of inverse weights cannot exceed 1")
        
        if total_inverse_weight < 1:
            redistribute_weights(*weights)
            # Add empty widgets to fill the remaining space
            self.children = [None] * len(self.weights)
    

    def add_widget(self, index, widget):
        if index < len(self.children):
            widget.width = self.get_child_width(index)
            self.children[index] = widget
            self.n_children += 1
        else:
            raise IndexError("Index out of range for widget insertion")

    def draw(self):
        # If no widgets are selected yet, select the first one
        first_child = self.children[0]
        if first_child is not None and self.selected and not any(child.selected for child in self.children if child is not None):
            first_child.selected = True

        y, x = self.term.get_location()
        for idx, child in enumerate(self.children):
            if child is not None:
                with self.term.location(x, y):
                    child.draw()
            x += self.get_child_width(idx)


    def get_child_width(self, index):
        return int(self.width / self.weights[index])

    def handle_input(self, user_input):
        if compare_input(user_input, self.term.KEY_LEFT):
            self.cursor_idx = (self.cursor_idx - 1) % self.n_children
        elif compare_input(user_input, self.term.KEY_RIGHT):
            self.cursor_idx = (self.cursor_idx + 1) % self.n_children
        elif compare_input(user_input, self.term.KEY_UP) or compare_input(user_input, self.term.KEY_BTAB) \
          or compare_input(user_input, self.term.KEY_DOWN) or compare_input(user_input, self.term.KEY_TAB):
            self.selected = False
        # Enter to activate the selected widget
        elif compare_input(user_input, self.term.KEY_ENTER):
            selected_widget = self.children[self.cursor_idx]
            if selected_widget is None:
                raise Exception("No widget selected")
            selected_widget.activate()

        # Ensure only one widget is selected at a time
        for i, widget in enumerate(self.children):
            if widget is None:
                continue
            if self.selected:
                widget.selected = i == self.cursor_idx
            else:
                widget.selected = False

        for child in self.children:
            if child is not None and child.selected:
                child.handle_input(user_input)
