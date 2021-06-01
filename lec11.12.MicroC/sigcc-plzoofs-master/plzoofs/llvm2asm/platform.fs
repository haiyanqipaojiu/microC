module Platform
(* -------------------------------------------------------------------------- *)
(** Assembling and linking for X86.  Depends on the underlying OS platform    *)
#nowarn "62"

open Printf
// open Unix

exception PlatformError of string * string


(* -------------------------------------------------------------------------- *)
(* Platform specific configuration: Unix/Linux vs. Mac OS X                   *)

// let os =
//     Sys.os_type (* One of "Unix" "Win32" or "Cygwin" *)

(* Default to Mac OS X configuration *)
open System.Runtime.InteropServices

let isLinux  = 
    RuntimeInformation.IsOSPlatform(OSPlatform.Linux)

let isWindows  = 
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

let isOSX  = 
    RuntimeInformation.IsOSPlatform(OSPlatform.OSX)

let linux = ref false

let linux_target_triple = ref "x86_64-suse-linux"


let mangle name =
    if isLinux || isWindows then
        name
    else
        ("_" ^ name)

let clang_cmd =
    ref "clang -fno-asynchronous-unwind-tables -S -mstackrealign -o "

let as_cmd =
    ref "clang -fno-asynchronous-unwind-tables -c -mstackrealign -o "

let link_cmd =
    ref "clang -fno-asynchronous-unwind-tables -mstackrealign -o "

let pp_cmd = ref "cpp -E "
let rm_cmd = ref "rm -rf "

let verbose = ref false
let verb msg = (if !verbose then (printf "%s" msg))


(* paths -------------------------------------------------------------------- *)

let path_sep = "/"

let dot_path = "./"
let output_path = ref "output"
let libs = ref []
let lib_paths = ref []
let lib_search_paths = ref []
let include_paths = ref []
let executable_name = ref "a.out"


(* Set the link commands properly, ensure output directory exists *)
let configure () =
    if isWindows then
             (verb "platform = windows\n"
              as_cmd := "gcc -c -o "
              link_cmd := "gcc -o ")
    else
        (if isLinux then
             (verb "platform = linux\n"
              as_cmd := "clang -c -o "
              link_cmd := "clang -o ")
         else
             (verb "platform = OS X\n"))
        //  try
        //      ignore (stat !output_path)
        //  with Unix_error (ENOENT, _, _) ->
        //      (verb
        //       <| Printf.sprintf "creating output directory: %s\n" !output_path)

        //      mkdir !output_path 0o755) //TODO

(* filename munging --------------------------------------------------------- *)
// let path_to_basename_ext (path: string) : string * string =
//     (* The path is of the form ... "foo/bar/baz/<file>.ext" *)
//     let paths =
//         Array.toList <| path.Split path_sep 

//     let _ =
//         if (List.length paths) = 0 then
//             failwith <| sprintf "bad path: %s" path in

//     let filename = List.head (List.rev paths) in

//     match Array.toList (filename.Split  ".")  with
//     | [ root ] -> root, ""
//     | [ root; ext ] -> printfn "%A" (root, ext); (root, ext)
//     | _ -> failwith <| sprintf "bad filename: %s" filename

(* compilation and shell commands-------------------------------------------- *)

(* Platform independent shell command *)
let sh (cmd: string) (ret: string -> int -> 'a) : 'a =
    verb (sprintf "* %s\n" cmd)
    ret cmd 0
    // match (system cmd) with
    // | WEXITED i -> ret cmd i
    // | WSIGNALED i -> raise (PlatformError(cmd, sprintf "Signaled with %d." i))
    // | WSTOPPED i -> raise (PlatformError(cmd, sprintf "Stopped with %d." i))
    //TODO

(* Generate a name that does not already exist.
   basedir includes the path separator
   ret.s
   ret_1.s
   ret_2.s
   ...
*)
let gen_name (basedir: string) (basen: string) (baseext: string) : string =
    let rec nocollide ofs =
        let nfn =
            (sprintf
                "%s/%s%s%s"
                basedir
                basen
                (if ofs = 0 then
                     ""
                 else
                     "_" ^ (string ofs))
                baseext)
        // if System.IO.File.Exists(nfn) then   nocollide (ofs + 1) else nfn //按序号生成新文件
        nfn //覆盖原文件
        
    nocollide 0


let raise_error cmd i =
    if i <> 0 then
        raise (PlatformError(cmd, sprintf "Exited with status %d." i))

    ()

let ignore_error _ _ = ()

let clang_compile (dot_ll: string) (dot_s: string) : unit =
    sh (sprintf "%s%s %s" !clang_cmd dot_s dot_ll) raise_error

let assemble (dot_s: string) (dot_o: string) : unit =
    sh (sprintf "%s%s %s" !as_cmd dot_o dot_s) raise_error

let preprocess (dot_oat: string) (dot_i: string) : unit =
    sh
        (sprintf "%s%s %s %s" !pp_cmd (List.fold (fun s i -> s ^ " -I" ^ i) "" !include_paths) dot_oat dot_i)
        raise_error

let link (mods: string list) (out_fn: string) : unit =
    sh
        (sprintf
            "%s%s %s %s %s %s"
            !link_cmd
            out_fn
            (String.concat " " (mods @ !lib_paths))
            (List.fold (fun s i -> s ^ " -L" ^ i) "" !lib_search_paths)
            (List.fold (fun s i -> s ^ " -I" ^ i) "" !include_paths)
            (List.fold (fun s l -> s ^ " -l" ^ l) "" !libs))
        raise_error
