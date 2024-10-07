extends Control

@onready var editor = $Editor

@onready var file_tab = $TabPanel/TabContainer/File
@onready var edit_tab = $TabPanel/TabContainer/Edit
@onready var view_tab = $TabPanel/TabContainer/View

@onready var save_dialog = $SaveDialog
@onready var load_dialog = $LoadDialog
@onready var folder_dialog = $FolderDialog

@onready var file_list = $FileList

@onready var current_file = null
@onready var current_folder = null

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	DisplayServer.window_set_title("CynVee-Code-Editor")
	
	save_dialog.current_dir = "/"
	load_dialog.current_dir = "/"
	
	for f in ["New", "Open", "Open Folder", "Save", "Save As..."]:
		if f:
			file_tab.get_popup().add_item(f)
		else:
			file_tab.get_popup().add_seperator()
	for e in ["Undo", "Redo", "Cut", "Copy", "Paste", "Select All"]:
		if e:
			edit_tab.get_popup().add_item(e)
		else:
			edit_tab.get_popup().add_seperator()
	for v in ["Line Number", "Minimap", "Fullscreen"]:
		if v:
			view_tab.get_popup().add_item(v)
		else:
			view_tab.get_popup().add_seperator()
	
	for control_highlight in ["if", "else", "elif", "switch", "match", "break"]:
		editor.syntax_highlighter.add_keyword_color (control_highlight, Color("#4c908e"))
	for keyword_highlight in ["var", "int", "string", "bool", "float", "double", "char", "func", "true", "false", "for", "in"]:
		editor.syntax_highlighter.add_keyword_color (keyword_highlight, Color("#936ebe"))
	
	var file_popup = file_tab.get_popup()
	var edit_popup = edit_tab.get_popup()
	var view_popup = view_tab.get_popup()
	
	file_popup.id_pressed.connect(file_menu)
	edit_popup.id_pressed.connect(edit_menu)
	view_popup.id_pressed.connect(view_menu)

func file_menu(id):
	match id:
		0:
			print("New")
			editor.clear()
			current_file = null
		1:
			print("Open")
			load_file()
		2:
			print("Open Folder")
			load_folder()
		3:
			print("Save")
			quick_save()
		4:
			print("Save As")
			save_file()

func edit_menu(id):
	match id:
		0:
			print("Undo")
			editor.undo()
		1:
			print("Redo")
			editor.redo()
		2:
			print("Cut")
			editor.cut()
		3:
			print("Copy")
			editor.copy()
		4:
			print("Paste")
			editor.paste()
		5:
			print("Select All")
			editor.select_all()

func view_menu(id):
	match id:
		0:
			print("Line Number")
		1:
			print("Minimap")
		2:
			print("Fullsreen")

func save_file():
	save_dialog.show()

func quick_save():
	if current_file != null:
		var file = FileAccess.open((current_file), FileAccess.WRITE)
		var content = editor.text
		file.store_string(content)
	else:
		pass

func load_file():
	load_dialog.show()

func load_folder():
	folder_dialog.show()

func _on_save_dialog_file_selected(path: String) -> void:
	var file = FileAccess.open((path), FileAccess.WRITE)
	var content = editor.text
	file.store_string(content)
	current_file = (path)

func _on_load_dialog_file_selected(path: String) -> void:
	print(path)
	var load_file = FileAccess.open((path), FileAccess.READ)
	editor.text = load_file.get_as_text()
	current_file = (path)
	file_list.add_item(current_file)

func _on_folder_dialog_dir_selected(dir: String) -> void:
	print(dir)
	current_folder = (dir)
	var open_dir = DirAccess.open(dir)
	var file_names = open_dir.get_files()
	for x in file_names:
		file_list.add_item(x)

func _on_file_list_item_activated(index: int) -> void:
	pass # Replace with function body.

func _on_editor_text_changed() -> void:
	var editor_text = editor.text

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
