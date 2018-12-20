import os
import struct

class block_data(object):
    def __init__(self, block_name, export_directory, recording_columns):
        self.recording_columns = recording_columns
        self.block_name = block_name
        self.column_index = 0
        if recording_columns:
            self.column_file = open(os.path.join(export_directory, block_name + '.txt'), 'w')

class binaryHelper(object):
    """Help with getting binary data from files"""

    def __init__(self, export_directory, file_content, file_index):
        self.export_dir = export_directory
        self.block_types = set()
        self.active_blocks = list()
        self.current_block = None
        self.file_content = file_content
        self.file_index = file_index
        self.file_length = len(file_content)

    def at_eof(self):
        return self.file_index >= self.file_length

    def start_block(self, block_type):
        if block_type in self.block_types:
            recording_columns = False
        else:
            recording_columns = True
            self.block_types.add(block_type)
        self.active_blocks.append(self.current_block)
        self.current_block = block_data(block_type, self.export_dir, recording_columns)

    def end_block(self):
        if self.current_block.recording_columns:
            self.current_block.column_file.close()
        self.current_block = self.active_blocks.pop()

    def record_column(self, column_label, data_type):
        if self.current_block.recording_columns:
            first_index = int(self.current_block.column_index / 26) - 1
            second_index = int(self.current_block.column_index % 26)
            column_id = chr(second_index + ord('A'))
            if first_index >= 0:
                column_id = chr(first_index + ord('A')) + column_id
            print('{0} {3} - {1} ({2})'.format(self.current_block.column_index, column_label, data_type, column_id), file=self.current_block.column_file)
            self.current_block.column_index += 1

    def read_block_header(self):
        block_header = struct.unpack('<4s', self.file_content[self.file_index:self.file_index+4])[0].decode('ASCII')
        self.file_index += 4
        return block_header

    def unwind_block_header(self):
        self.file_index -= 4

    def read_int16(self, column_label, csv_file):
        self.record_column(column_label, 'int16')
        value_read = struct.unpack('<h', self.file_content[self.file_index:self.file_index+2])[0]
        self.file_index += 2
        csv_file.write(str(value_read) + ',')
        return value_read

    def read_int32(self, column_label, csv_file):
        self.record_column(column_label, 'int32')
        value_read = struct.unpack('<i', self.file_content[self.file_index:self.file_index+4])[0]
        csv_file.write(str(value_read) + ',')
        self.file_index += 4
        return value_read

    def read_coded_int32(self, column_label, csv_file):
        self.record_column(column_label, 'int32coded')
        value1 = struct.unpack('<h', self.file_content[self.file_index:self.file_index+2])[0]
        self.file_index += 2
        value2 = struct.unpack('<h', self.file_content[self.file_index:self.file_index+2])[0]
        self.file_index += 2
        calc_value = (value2 * 32768) + value1;
        csv_file.write(str(calc_value) + ',')
        return calc_value;

    def read_string(self, column_label, csv_file):
        self.record_column(column_label + ' Length', 'int16')
        self.record_column(column_label, 'string')
        string_length = struct.unpack('<H', self.file_content[self.file_index:self.file_index+2])[0]
        csv_file.write(str(string_length) + ',')
        self.file_index += 2
        value_string = struct.unpack('<'+str(string_length)+'s', self.file_content[self.file_index:self.file_index+string_length])[0].decode('ASCII')
        csv_file.write('"' + str(value_string) + '",')
        self.file_index += string_length
        return value_string
