import blessed

class Widget:
    """An interselected component in a view, e.g. a TextField."""

    def __init__(self, controller):
        self.controller = controller
        self.term = controller.term
        self.selected = False

    def activate(self):
        self.selected = True

    def draw(self):
        pass

    def handle_input(self, user_input):
        pass


class Button(Widget):
    def __init__(self, controller, text, action, *params):
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

class TextField(Widget):
    def __init__(self, controller, label, censored=False):
        super().__init__(controller)
        self.label = label
        self.text = ''
        self.censored = censored

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
        if user_input.is_sequence:
            if user_input.code == self.term.KEY_BACKSPACE:
                self.text = self.text[:-1]
        else:
            self.text += user_input