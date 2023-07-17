from typing import Callable
from input import compare_input
from blessed import Terminal

class Widget:
    """An interselected component in a view, e.g. a TextField."""

    def __init__(self, controller):
        self.controller = controller
        self.term: Terminal = controller.term
        self.selected = False

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

    def activate(self):
        super().activate()
        self.action(*self.params)

    def draw(self):
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
    
    def draw(self):
        if self.checked:
            state = "[x]"
        else:
            state = "[ ]"

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
        self.box_width = self.controller.term.width - 2 # Subtract 2 for the box borders

    def draw(self):
        super().draw()
        print_row = lambda row: print(row)
        if self.selected:
            print_row = lambda row: print(self.term.yellow(row))

        print_row('┌' + '─' * self.box_width + '┐')
        
        for i in range(self._top_line_idx, min(self._top_line_idx + self.num_lines, len(self.lines))):
            line = self.lines[i]

            # If the line is shorter than box_width, pad it with spaces
            if len(line) < self.box_width:
                line += ' ' * (self.box_width - len(line))
            # If the line is longer than box_width, truncate it
            elif len(line) > self.box_width:
                line = line[:self.box_width]

            print_row('│' + line + '│')
        
        # Fill in empty lines if there are less than num_lines
        for _ in range(len(self.lines), self._top_line_idx + self.num_lines):
            print_row('│' + ' ' * self.box_width + '│')

        print_row('└' + '─' * self.box_width + '┘')

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