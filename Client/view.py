class View:
    def __init__(self, controller):
        self.controller = controller
        self.term = self.controller.term
        self.dirty = True # If True, the view will be redrawn on the next draw() call

    def redraw_if_dirty(self):
        if self.dirty:
            print(self.term.clear)
            self.draw()
            self.dirty = False

    def draw(self):
        pass

class MenuView(View):
    def __init__(self, controller, title=''):
        super().__init__(controller)

        # title shows up at the top of the menu
        self.title: str = title

    def draw(self):
        super().draw()
        # If no widgets are selected yet, select the first one
        if not any(widget.selected for widget in self.controller.widgets):
            self.controller.widgets[0].selected = True
        print(self.term.clear)
        print(self.term.center(self.title))
        print(self.term.move_down)
        for widget in self.controller.widgets:
            widget.draw()

class MainMenuView(MenuView):
    def __init__(self, controller):
        super().__init__(controller, title='moonlapseMUD')

class UserMenuView(MenuView):
    def __init__(self, controller, title=''):
        super().__init__(controller, title=title)


class LoginMenuView(UserMenuView):
    def __init__(self, controller):
        super().__init__(controller, title='Login')


class RegisterMenuView(UserMenuView):
    def __init__(self, controller):
        super().__init__(controller, title='Register')

class ChatroomView(View):
    def __init__(self, controller):
        super().__init__(controller)
        self.controller = controller

    def draw(self):
        super().draw()
        # Draw the chat log
        for i, message in enumerate(self.controller.chat_log):
            print(self.controller.term.move_y(i) + message)

        # Draw the widgets
        for widget in self.controller.widgets:
            widget.draw()

        print(self.controller.term.move_y(len(self.controller.chat_log) + len(self.controller.widgets)))
